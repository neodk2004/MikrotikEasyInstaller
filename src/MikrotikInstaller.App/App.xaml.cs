using System.Windows;
using System.Windows.Media;
using Wpf.Ui.Appearance;

namespace MikrotikInstaller.App;

public partial class App : Application
{
    /// <summary>MikroTik-Markenfarbe (aus mikrotik.com übernommen) als Akzent für alle "Primary"-Elemente.</summary>
    public static readonly Color BrandAccent = Color.FromRgb(0x00, 0x92, 0x45);

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Übernimmt Hell/Dunkel vom Windows-Systemthema; die Akzentfarbe setzen wir
        // anschließend selbst auf MikroTik-Grün statt auf die Windows-Akzentfarbe.
        ApplicationThemeManager.ApplySystemTheme(updateAccent: false);
        var appliedTheme = ApplicationThemeManager.GetAppTheme();
        ApplicationAccentColorManager.Apply(BrandAccent, appliedTheme, systemGlassColor: false, systemAccentColor: false);
    }
}
