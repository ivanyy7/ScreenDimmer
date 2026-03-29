namespace ScreenDimmer;

/// <summary>
/// Пользовательские настройки окна (подэтап 2.4). Сериализуются в JSON.
/// </summary>
public sealed class UserSettingsData
{
    public bool IsDarkTheme { get; set; } = true;

    public double? WindowLeft { get; set; }
    public double? WindowTop { get; set; }
    public double? WindowWidth { get; set; }
    public double? WindowHeight { get; set; }
}
