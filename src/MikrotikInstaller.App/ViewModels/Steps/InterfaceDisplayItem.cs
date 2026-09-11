using MikrotikInstaller.Core.Devices;

namespace MikrotikInstaller.App.ViewModels.Steps;

/// <summary>Aufbereitete Anzeige einer Schnittstelle für die Geräte-Übersicht (laientaugliche Texte statt Enum-Werte).</summary>
public sealed record InterfaceDisplayItem(string Name, string KindLabel, string StatusLabel, bool IsProblem)
{
    public static InterfaceDisplayItem From(InterfaceInfo info)
    {
        var kindLabel = info.Kind switch
        {
            InterfaceKind.Ethernet => "Ethernet",
            InterfaceKind.Bridge => "Bridge",
            InterfaceKind.Vlan => "VLAN",
            InterfaceKind.Wireless => "WLAN",
            _ => "Sonstige",
        };

        var statusLabel = info.IsDisabled ? "Deaktiviert" : info.IsRunning ? "Aktiv" : "Inaktiv";

        return new InterfaceDisplayItem(info.Name, kindLabel, statusLabel, IsProblem: info.IsDisabled);
    }
}
