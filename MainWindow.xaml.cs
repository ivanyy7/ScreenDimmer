using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MediaBrushes = System.Windows.Media.Brushes;
using MediaColor = System.Windows.Media.Color;

namespace ScreenDimmer;

/// <summary>
/// Главное информационное окно (этап 2).
/// Подэтап 2.4 — сохранение темы и геометрии в %LocalAppData%\ScreenDimmer\settings.json
/// </summary>
public partial class MainWindow : Window
{
    private const double SaveDebounceMs = 400;

    private bool _settingsLoaded;
    private bool _suppressSettingsPersist;
    private DispatcherTimer? _saveDebounceTimer;

    public MainWindow()
    {
        InitializeComponent();
        ThemeDarkRadio.Checked += ThemeRadio_OnChecked;
        ThemeLightRadio.Checked += ThemeRadio_OnChecked;
        LocationChanged += (_, _) => SchedulePersistSettings();
        SizeChanged += (_, _) => SchedulePersistSettings();
        Loaded += MainWindow_OnLoaded;
        IsVisibleChanged += MainWindow_OnIsVisibleChanged;

        LoadSettingsFromDisk();
    }

    private void LoadSettingsFromDisk()
    {
        _suppressSettingsPersist = true;
        try
        {
            var data = UserSettingsStore.TryLoad() ?? new UserSettingsData();

            ApplyTheme(data.IsDarkTheme);
            if (data.IsDarkTheme)
            {
                ThemeDarkRadio.IsChecked = true;
            }
            else
            {
                ThemeLightRadio.IsChecked = true;
            }

            if (data.WindowWidth is > 0)
                Width = Math.Max(MinWidth, data.WindowWidth.Value);
            if (data.WindowHeight is > 0)
                Height = Math.Max(MinHeight, data.WindowHeight.Value);
            if (data.WindowLeft.HasValue)
                Left = data.WindowLeft.Value;
            if (data.WindowTop.HasValue)
                Top = data.WindowTop.Value;
        }
        finally
        {
            _suppressSettingsPersist = false;
            _settingsLoaded = true;
        }
    }

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        ClampWindowToVirtualScreen();
    }

    private void MainWindow_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (IsLoaded && !IsVisible && _settingsLoaded && !_suppressSettingsPersist)
            PersistSettings();
    }

    private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }

    private void MainWindow_OnStateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            WindowState = WindowState.Normal;
            Hide();
        }
    }

    private void TitleBar_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            try
            {
                DragMove();
            }
            catch
            {
                // DragMove может бросить, если окно ещё не готово — игнорируем.
            }
        }
    }

    private void MinimizeToTray_OnClick(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    private void ThemeRadio_OnChecked(object sender, RoutedEventArgs e)
    {
        ApplyTheme(ThemeDarkRadio.IsChecked == true);
        if (_suppressSettingsPersist || !_settingsLoaded)
            return;
        PersistSettings();
    }

    private void SchedulePersistSettings()
    {
        if (!_settingsLoaded || _suppressSettingsPersist)
            return;

        _saveDebounceTimer ??= new DispatcherTimer(
            TimeSpan.FromMilliseconds(SaveDebounceMs),
            DispatcherPriority.Background,
            OnSaveDebounceTick,
            Dispatcher);

        _saveDebounceTimer.Stop();
        _saveDebounceTimer.Start();
    }

    private void OnSaveDebounceTick(object? sender, EventArgs e)
    {
        _saveDebounceTimer?.Stop();
        PersistSettings();
    }

    private void PersistSettings()
    {
        if (!_settingsLoaded || _suppressSettingsPersist)
            return;

        var data = new UserSettingsData
        {
            IsDarkTheme = ThemeDarkRadio.IsChecked == true,
            WindowLeft = Left,
            WindowTop = Top,
            WindowWidth = Width,
            WindowHeight = Height,
        };
        UserSettingsStore.Save(data);
    }

    /// <summary>
    /// Удерживает окно в пределах виртуального экрана (все мониторы).
    /// </summary>
    private void ClampWindowToVirtualScreen()
    {
        double vLeft = SystemParameters.VirtualScreenLeft;
        double vTop = SystemParameters.VirtualScreenTop;
        double vWidth = SystemParameters.VirtualScreenWidth;
        double vHeight = SystemParameters.VirtualScreenHeight;

        double w = ActualWidth > 0 ? ActualWidth : Width;
        double h = ActualHeight > 0 ? ActualHeight : Height;

        if (w > vWidth)
        {
            Width = Math.Max(MinWidth, vWidth);
            w = Width;
        }

        if (h > vHeight)
        {
            Height = Math.Max(MinHeight, vHeight);
            h = Height;
        }

        if (Left < vLeft)
            Left = vLeft;
        if (Top < vTop)
            Top = vTop;
        if (Left + w > vLeft + vWidth)
            Left = vLeft + vWidth - w;
        if (Top + h > vTop + vHeight)
            Top = vTop + vHeight - h;
    }

    /// <summary>
    /// Применяет тёмную или светлую палитру без анимаций; контраст достаточен для чтения.
    /// </summary>
    private void ApplyTheme(bool isDark)
    {
        if (isDark)
        {
            Background = new SolidColorBrush(MediaColor.FromRgb(0x2D, 0x2D, 0x30));
            ChromeBorder.BorderBrush = new SolidColorBrush(MediaColor.FromRgb(0x55, 0x55, 0x55));
            TitleBarBorder.Background = new SolidColorBrush(MediaColor.FromRgb(0x3C, 0x3C, 0x3C));
            TitleBarText.Foreground = MediaBrushes.White;
            MinimizeToTrayButton.Foreground = MediaBrushes.White;
            HeadingTextBlock.Foreground = MediaBrushes.White;
            DescTextBlock.Foreground = new SolidColorBrush(MediaColor.FromRgb(0xF0, 0xF0, 0xF0));
            HotkeyTextBlock.Foreground = new SolidColorBrush(MediaColor.FromRgb(0xF0, 0xF0, 0xF0));
            ThemeLabelText.Foreground = new SolidColorBrush(MediaColor.FromRgb(0xE0, 0xE0, 0xE0));
            ThemeDarkRadio.Foreground = new SolidColorBrush(MediaColor.FromRgb(0xE0, 0xE0, 0xE0));
            ThemeLightRadio.Foreground = new SolidColorBrush(MediaColor.FromRgb(0xE0, 0xE0, 0xE0));
            ContentScrollViewer.Background = MediaBrushes.Transparent;
        }
        else
        {
            Background = new SolidColorBrush(MediaColor.FromRgb(0xF5, 0xF5, 0xF5));
            ChromeBorder.BorderBrush = new SolidColorBrush(MediaColor.FromRgb(0xA0, 0xA0, 0xA0));
            TitleBarBorder.Background = new SolidColorBrush(MediaColor.FromRgb(0xE8, 0xE8, 0xE8));
            TitleBarText.Foreground = new SolidColorBrush(MediaColor.FromRgb(0x1E, 0x1E, 0x1E));
            MinimizeToTrayButton.Foreground = new SolidColorBrush(MediaColor.FromRgb(0x1E, 0x1E, 0x1E));
            HeadingTextBlock.Foreground = new SolidColorBrush(MediaColor.FromRgb(0x1E, 0x1E, 0x1E));
            DescTextBlock.Foreground = new SolidColorBrush(MediaColor.FromRgb(0x33, 0x33, 0x33));
            HotkeyTextBlock.Foreground = new SolidColorBrush(MediaColor.FromRgb(0x33, 0x33, 0x33));
            ThemeLabelText.Foreground = new SolidColorBrush(MediaColor.FromRgb(0x33, 0x33, 0x33));
            ThemeDarkRadio.Foreground = new SolidColorBrush(MediaColor.FromRgb(0x33, 0x33, 0x33));
            ThemeLightRadio.Foreground = new SolidColorBrush(MediaColor.FromRgb(0x33, 0x33, 0x33));
            ContentScrollViewer.Background = new SolidColorBrush(MediaColor.FromRgb(0xF5, 0xF5, 0xF5));
        }
    }
}
