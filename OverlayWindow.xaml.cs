using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Forms;

namespace ScreenDimmer;

/// <summary>
/// Полноэкранное чёрное окно на одном мониторе. Позиция задаётся в пикселях через Win32 (корректно при любом DPI).
/// </summary>
public partial class OverlayWindow : Window
{
    private const uint SwpNoactivate = 0x0010;
    private const uint SwpShowwindow = 0x0040;
    private static readonly IntPtr HwndTopmost = new(-1);

    public OverlayWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Скрываем из Alt+Tab (дополнительно к ShowInTaskbar=False).
        var helper = new WindowInteropHelper(this);
        int ex = GetWindowLong(helper.Handle, GwlExstyle);
        SetWindowLong(helper.Handle, GwlExstyle, ex | WsExToolwindow);
    }

    /// <summary>Разместить окно ровно по границам указанного монитора (включая область панели задач).</summary>
    public void PositionOnScreen(Screen screen)
    {
        var b = screen.Bounds;
        var helper = new WindowInteropHelper(this);
        helper.EnsureHandle();
        SetWindowPos(
            helper.Handle,
            HwndTopmost,
            b.Left,
            b.Top,
            b.Width,
            b.Height,
            SwpNoactivate | SwpShowwindow);
    }

    private const int GwlExstyle = -20;
    private const int WsExToolwindow = 0x00000080;

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
}
