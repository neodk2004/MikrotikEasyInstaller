namespace MikrotikInstaller.Core.Devices;

/// <summary>
/// Grob vereinfachte Klassifizierung einer RouterOS-Schnittstelle für den Assistenten
/// (z. B. um zu entscheiden, ob der WLAN-Schritt überhaupt angezeigt werden muss).
/// </summary>
public enum InterfaceKind
{
    Ethernet,
    Bridge,
    Vlan,
    Wireless,
    Other,
}
