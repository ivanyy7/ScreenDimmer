using System.IO;
using System.Runtime.InteropServices;

namespace ScreenDimmer;

/// <summary>
/// Автозапуск через ярлык в папке «Автозагрузка» пользователя (без записи в реестр HKLM).
/// </summary>
public static class AutostartService
{
    private static string ShortcutPath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Startup),
            "ScreenDimmer.lnk");

    /// <summary>Есть ли ярлык автозапуска.</summary>
    public static bool IsEnabled => File.Exists(ShortcutPath);

    /// <summary>Включить или выключить автозапуск (создание/удаление ярлыка).</summary>
    public static void SetEnabled(bool enabled)
    {
        if (!enabled)
        {
            try
            {
                if (File.Exists(ShortcutPath))
                    File.Delete(ShortcutPath);
            }
            catch
            {
                // Игнорируем отказ в доступе и т.п.
            }

            return;
        }

        var exe = Environment.ProcessPath;
        if (string.IsNullOrEmpty(exe) || !File.Exists(exe))
            return;

        try
        {
            CreateShortcut(exe!);
        }
        catch
        {
            // COM/политики — без падения приложения.
        }
    }

    private static void CreateShortcut(string targetPath)
    {
        var t = Type.GetTypeFromProgID("WScript.Shell");
        if (t == null)
            return;

        dynamic? wsh = null;
        try
        {
            wsh = Activator.CreateInstance(t);
            dynamic sc = wsh!.CreateShortcut(ShortcutPath);
            sc.TargetPath = targetPath;
            sc.WorkingDirectory = Path.GetDirectoryName(targetPath);
            sc.Description = "ScreenDimmer — затемнение экранов";
            sc.Save();
        }
        finally
        {
            if (wsh != null)
                Marshal.FinalReleaseComObject(wsh);
        }
    }
}
