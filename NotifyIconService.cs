using System.Windows.Forms;

namespace ScreenDimmer;

/// <summary>
/// Иконка в области уведомлений и контекстное меню.
/// </summary>
public sealed class NotifyIconService : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _contextMenu;
    private readonly ToolStripMenuItem _itemAutostart;

    public event Action? ToggleRequested;
    public event Action? ExitRequested;
    /// <summary>Подэтап 2.6: левый клик по иконке — показать/скрыть главное окно (обработка в WPF через Dispatcher).</summary>
    public event Action? MainWindowToggleRequested;

    public NotifyIconService()
    {
        var trayIcon = TrayIconLoader.LoadBestAvailable();

        var itemToggle = new ToolStripMenuItem("Toggle Dimmer (F8)");
        itemToggle.Click += (_, _) => ToggleRequested?.Invoke();

        _itemAutostart = new ToolStripMenuItem("Автозапуск при входе в Windows")
        {
            CheckOnClick = true,
            Checked = AutostartService.IsEnabled,
        };
        _itemAutostart.Click += OnAutostartClick;

        var itemExit = new ToolStripMenuItem("Exit");
        itemExit.Click += (_, _) => ExitRequested?.Invoke();

        _contextMenu = new ContextMenuStrip();
        _contextMenu.Items.Add(itemToggle);
        _contextMenu.Items.Add(new ToolStripSeparator());
        _contextMenu.Items.Add(_itemAutostart);
        _contextMenu.Items.Add(itemExit);

        _notifyIcon = new NotifyIcon
        {
            Icon = trayIcon,
            Text = "ScreenDimmer — F8 затемнение; ЛКМ — окно настроек",
            Visible = true,
            ContextMenuStrip = _contextMenu,
        };
        // ЛКМ: MouseDown, а не MouseClick — в трее (в т.ч. у «скрытых» иконок) пары Down/Up часто
        // не распознаются как полный клик, и MouseClick не вызывается.
        _notifyIcon.MouseDown += OnNotifyIconMouseDown;
    }

    private void OnNotifyIconMouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
            MainWindowToggleRequested?.Invoke();
    }

    private void OnAutostartClick(object? sender, EventArgs e)
    {
        // При CheckOnClick флажок уже переключён к моменту Click.
        AutostartService.SetEnabled(_itemAutostart.Checked);
        _itemAutostart.Checked = AutostartService.IsEnabled;
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _contextMenu.Dispose();
    }
}
