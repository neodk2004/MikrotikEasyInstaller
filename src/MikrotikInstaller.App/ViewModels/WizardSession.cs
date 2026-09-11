using MikrotikInstaller.Core.Configuration;
using MikrotikInstaller.Core.Connectivity;
using MikrotikInstaller.Core.Devices;

namespace MikrotikInstaller.App.ViewModels;

/// <summary>
/// Über den gesamten Assistenten geteilter Zustand: die einmal hergestellte Geräteverbindung,
/// die daraus abgeleiteten Erkenntnisse und die von jedem Schritt geplanten Änderungen.
/// Nichts davon wird angewendet, bevor der Zusammenfassungs-Schritt es tut.
/// </summary>
public sealed class WizardSession
{
    public IRouterOsClient? Client { get; set; }

    public RouterDevice? Device { get; set; }

    /// <summary>Vom WAN-Schritt gewählte Internet-Schnittstelle, damit spätere Schritte sie referenzieren können.</summary>
    public string? WanInterfaceName { get; set; }

    /// <summary>Vom LAN-Schritt gewählter Bridge-Name, damit VLAN- und Firewall-Schritt ihn referenzieren können.</summary>
    public string? LanBridgeName { get; set; }

    public IReadOnlyList<ConfigurationAction>? WanActions { get; set; }

    public IReadOnlyList<ConfigurationAction>? LanActions { get; set; }

    public IReadOnlyList<ConfigurationAction>? VlanActions { get; set; }

    public IReadOnlyList<ConfigurationAction>? WirelessActions { get; set; }

    public IReadOnlyList<ConfigurationAction>? FirewallActions { get; set; }

    // Die rohen Eingaben je Schritt, damit der Zusammenfassungs-Schritt daraus eine
    // laientaugliche Klartext-Vorschau bauen kann (getrennt von den technischen
    // ConfigurationAction-Beschreibungen, die beim Anwenden als Fortschrittstext dienen).
    public WanSettings? WanSettings { get; set; }

    public LanSettings? LanSettings { get; set; }

    public IReadOnlyList<VlanDefinition>? VlanDefinitions { get; set; }

    public WirelessSettings? WirelessSettings { get; set; }

    public FirewallSettings? FirewallSettings { get; set; }

    public async Task DisposeClientAsync()
    {
        if (Client is not null)
        {
            await Client.DisposeAsync();
            Client = null;
        }
    }
}
