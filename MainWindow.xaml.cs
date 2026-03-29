using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MediaBrushes = System.Windows.Media.Brushes;
using MediaColor = System.Windows.Media.Color;

namespace ScreenDimmer;

/// <summary>
/// Главное информационное окно (этап 2).
/// Подэтап 2.3 — переключение тёмной/светлой темы в рамках сессии (сохранение — подэтап 2.4).
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ApplyTheme(isDark: true);
        ThemeDarkRadio.Checked += ThemeRadio_OnChecked;
        ThemeLightRadio.Checked += ThemeRadio_OnChecked;
    }

    private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        // Не даём уничтожить окно и процесс — только скрываем (выход — из меню трея).
        e.Cancel = true;
        Hide();
    }

    private void MainWindow_OnStateChanged(object? sender, EventArgs e)
    {
        // Если окно каким-то образом ушло в Minimize — прячем в трей, без кнопки в панели задач.
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
