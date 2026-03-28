using System.Windows.Forms;

namespace ScreenDimmer;

/// <summary>
/// Иконка в области уведомлений и контекстное меню.
/// </summary>
public sealed class NotifyIconService : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _contextMenu;

    public event Action? ToggleRequested;
    public event Action? ExitRequested;

    public NotifyIconService()
    {
        var itemToggle = new ToolStripMenuItem("Toggle Dimmer (F8)");
        itemToggle.Click += (_, _) => ToggleRequested?.Invoke();

        var itemExit = new ToolStripMenuItem("Exit");
        itemExit.Click += (_, _) => ExitRequested?.Invoke();

        _contextMenu = new ContextMenuStrip();
        _contextMenu.Items.Add(itemToggle);
        _contextMenu.Items.Add(itemExit);

        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "ScreenDimmer — F8 для переключения",
            Visible = true,
            ContextMenuStrip = _contextMenu,
        };
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _contextMenu.Dispose();
    }
}
