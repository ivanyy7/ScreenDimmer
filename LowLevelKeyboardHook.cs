using System.Runtime.InteropServices;

namespace ScreenDimmer;

/// <summary>
/// Глобальный низкоуровневый хук клавиатуры (WH_KEYBOARD_LL).
/// RegisterHotKey не видит Fn — поэтому перехват F8 идёт здесь (часто Fn+F8 с клавиатуры приходит как отдельное событие F8 / с флагом extended — зависит от OEM).
/// </summary>
public sealed class LowLevelKeyboardHook : IDisposable
{
    private const int WhKeyboardLl = 13;

    private const int WmKeyup = 0x0101;
    private const int WmSyskeyup = 0x0107;

    private IntPtr _hookHandle = IntPtr.Zero;
    private readonly HookProc _hookProc;

    /// <summary>Срабатывает при отпускании F8 (один раз на физическое нажатие — без автоповтора).</summary>
    public event EventHandler? F8KeyReleased;

    public LowLevelKeyboardHook()
    {
        // Держим делегат в поле, иначе GC собьёт хук.
        _hookProc = HookCallback;
    }

    public void Install()
    {
        if (_hookHandle != IntPtr.Zero)
            return;

        // Для WH_KEYBOARD_LL в текущем процессе допустим модуль EXE (GetModuleHandle(null)).
        IntPtr hMod = GetModuleHandle(null);
        _hookHandle = SetWindowsHookEx(WhKeyboardLl, _hookProc, hMod, 0);
        if (_hookHandle == IntPtr.Zero)
            throw new InvalidOperationException("SetWindowsHookEx(WH_KEYBOARD_LL) вернул NULL.");
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && lParam != IntPtr.Zero)
        {
            var info = Marshal.PtrToStructure<Kbdllhookstruct>(lParam);
            if (info.VkCode == 0x77) // VK_F8
            {
                int msg = wParam.ToInt32();
                if (msg == WmKeyup || msg == WmSyskeyup)
                    F8KeyReleased?.Invoke(this, EventArgs.Empty);
            }
        }

        return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }

    public void Dispose()
    {
        if (_hookHandle != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hookHandle);
            _hookHandle = IntPtr.Zero;
        }
    }

    private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct Kbdllhookstruct
    {
        public uint VkCode;
        public uint ScanCode;
        public uint Flags;
        public uint Time;
        public UIntPtr DwExtraInfo;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);
}
