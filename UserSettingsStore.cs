using System.IO;
using System.Text.Json;

namespace ScreenDimmer;

/// <summary>
/// Сохранение настроек в %LocalAppData%\ScreenDimmer\settings.json
/// </summary>
internal static class UserSettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    private static string GetFilePath()
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ScreenDimmer");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "settings.json");
    }

    public static UserSettingsData? TryLoad()
    {
        try
        {
            var path = GetFilePath();
            if (!File.Exists(path))
                return null;
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<UserSettingsData>(json, JsonOptions)
                   ?? new UserSettingsData();
        }
        catch
        {
            return null;
        }
    }

    public static void Save(UserSettingsData data)
    {
        try
        {
            var path = GetFilePath();
            var json = JsonSerializer.Serialize(data, JsonOptions);
            File.WriteAllText(path, json);
        }
        catch
        {
            // Не блокируем UI при ошибке диска/прав.
        }
    }
}
