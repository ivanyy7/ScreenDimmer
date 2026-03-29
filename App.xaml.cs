using System.Threading;
using System.Windows;

namespace ScreenDimmer;

/// <summary>
/// Точка входа: один экземпляр, трей, хук клавиатуры, сервис затемнения.
/// </summary>
public partial class App : System.Windows.Application
{
    private const string MutexName = "Local\\ScreenDimmer_SingleInstance_7E2A9B1C-4D5E-4F60-8A3B-2C9D1E0F5A6B";

    private Mutex? _singleInstanceMutex;
    private NotifyIconService? _tray;
    private LowLevelKeyboardHook? _keyboardHook;
    private DimmerService? _dimmer;
    private MainWindow? _mainWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        // Только один процесс (user-mode mutex).
        var mutex = new Mutex(true, MutexName, out bool createdNew);
        if (!createdNew)
        {
            mutex.Dispose();
            Shutdown();
            return;
        }

        _singleInstanceMutex = mutex;

        base.OnStartup(e);

        _dimmer = new DimmerService();
        _keyboardHook = new LowLevelKeyboardHook();
        _keyboardHook.F8KeyReleased += OnF8KeyReleased;
        _keyboardHook.Install();

        _tray = new NotifyIconService();
        _tray.ToggleRequested += () => Dispatcher.Invoke(ToggleDimmerSafe);
        _tray.ExitRequested += () => Dispatcher.InvokeShutdown();
        _tray.MainWindowToggleRequested += () => Dispatcher.BeginInvoke(ToggleMainWindowSafe);

        // Подэтап 2.5–2.6: окно создаётся в коде, при старте скрыто; ЛКМ по иконке трея — показ/скрытие.
        _mainWindow = new MainWindow();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            _keyboardHook?.Dispose();
        }
        catch
        {
            // Игнорируем ошибки при завершении процесса.
        }

        _tray?.Dispose();
        _dimmer?.Dispose();
        _singleInstanceMutex?.ReleaseMutex();
        _singleInstanceMutex?.Dispose();

        base.OnExit(e);
    }

    /// <summary>
    /// Событие от низкоуровневого хука приходит не из UI-потока — переключение только через Dispatcher.
    /// </summary>
    private void OnF8KeyReleased(object? sender, EventArgs e)
    {
        Dispatcher.BeginInvoke(new Action(ToggleDimmerSafe));
    }

    private void ToggleDimmerSafe()
    {
        try
        {
            _dimmer?.Toggle();
        }
        catch (Exception ex)
        {
            // Не роняем процесс из-за ошибки оверлея.
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    /// <summary>
    /// Подэтап 2.6: WinForms трей вызывает из фона — переключение окна только на UI-потоке WPF.
    /// </summary>
    private void ToggleMainWindowSafe()
    {
        if (_mainWindow == null)
            return;
        if (_mainWindow.IsVisible)
        {
            _mainWindow.Hide();
        }
        else
        {
            _mainWindow.Show();
            _mainWindow.WindowState = WindowState.Normal;
            // Без краткого Topmost окно часто остаётся под другими после клика по трею (ограничения foreground).
            _mainWindow.Topmost = true;
            _mainWindow.Activate();
            _mainWindow.Topmost = false;
        }
    }
}
