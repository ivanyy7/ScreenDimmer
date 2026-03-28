using System.Windows;
using System.Windows.Forms;

namespace ScreenDimmer;

/// <summary>
/// Управление оверлеями: по одному полноэкранному окну на каждый монитор из <see cref="Screen.AllScreens"/>.
/// </summary>
public sealed class DimmerService : IDisposable
{
    private readonly object _sync = new();
    private List<OverlayWindow> _windows = new();
    private bool _dimmed;

    public bool IsDimmed
    {
        get
        {
            lock (_sync)
                return _dimmed;
        }
    }

    /// <summary>Переключить: затемнение всех мониторов ↔ обычный режим.</summary>
    public void Toggle()
    {
        lock (_sync)
        {
            if (_dimmed)
                HideAllInternal();
            else
                ShowAllInternal();
        }
    }

    private void ShowAllInternal()
    {
        RebuildWindowsIfNeeded();

        var screens = Screen.AllScreens;
        for (int i = 0; i < _windows.Count; i++)
        {
            if (i < screens.Length)
                _windows[i].PositionOnScreen(screens[i]);
            _windows[i].Visibility = Visibility.Visible;
            _windows[i].Show();
        }

        _dimmed = true;
    }

    private void HideAllInternal()
    {
        foreach (var w in _windows)
        {
            w.Hide();
            w.Visibility = Visibility.Hidden;
        }

        _dimmed = false;
    }

    /// <summary>Пересоздать окна, если изменилось число мониторов или список пуст.</summary>
    private void RebuildWindowsIfNeeded()
    {
        var screens = Screen.AllScreens;
        if (_windows.Count == screens.Length && _windows.Count > 0)
        {
            for (int i = 0; i < screens.Length; i++)
                _windows[i].PositionOnScreen(screens[i]);
            return;
        }

        foreach (var old in _windows)
        {
            old.Close();
        }

        _windows = new List<OverlayWindow>(screens.Length);
        foreach (var s in screens)
        {
            var w = new OverlayWindow();
            w.PositionOnScreen(s);
            _windows.Add(w);
        }
    }

    public void Dispose()
    {
        lock (_sync)
        {
            foreach (var w in _windows)
            {
                try
                {
                    w.Close();
                }
                catch
                {
                    // ignored
                }
            }

            _windows.Clear();
            _dimmed = false;
        }
    }
}
