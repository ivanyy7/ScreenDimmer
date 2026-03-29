using System.Drawing;
using System.IO;

namespace ScreenDimmer;

/// <summary>
/// Загрузка иконки трея: встроенный ресурс <c>Assets/app.ico</c>, иначе файл рядом с exe.
/// ICO должен быть в формате, принимаемом GDI+ (см. <c>tools/rebuild-app-ico.ps1</c>).
/// </summary>
internal static class TrayIconLoader
{
    public static Icon LoadBestAvailable()
    {
        var asm = typeof(TrayIconLoader).Assembly;
        var names = asm.GetManifestResourceNames();

        foreach (var name in names ?? Array.Empty<string>())
        {
            if (!name.EndsWith("app.ico", StringComparison.OrdinalIgnoreCase))
                continue;
            try
            {
                var s = asm.GetManifestResourceStream(name);
                if (s != null)
                    return new Icon(s);
            }
            catch
            {
                // пробуем файл на диске
            }
        }

        var icoDisk = Path.Combine(AppContext.BaseDirectory, "Assets", "app.ico");
        try
        {
            if (File.Exists(icoDisk))
                return new Icon(icoDisk);
        }
        catch
        {
            // ignored
        }

        return (Icon)SystemIcons.Application.Clone();
    }
}
