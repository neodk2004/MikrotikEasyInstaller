namespace MikrotikInstaller.App.ViewModels.Steps;

/// <summary>
/// Platzhalter für Schritte, die in späteren Phasen mit echter Logik gefüllt werden
/// (Geräte-Erkennung, Internet/WAN, DHCP/LAN, VLAN, WLAN, Firewall, Zusammenfassung).
/// Dient aktuell dazu, die Navigation des Assistenten durchgängig testbar zu machen.
/// </summary>
public partial class PlaceholderStepViewModel : WizardStepViewModelBase
{
    public PlaceholderStepViewModel(string title, string description)
        : base(title, description)
    {
    }
}
