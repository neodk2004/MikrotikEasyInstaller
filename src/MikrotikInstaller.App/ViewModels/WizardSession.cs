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

    /// <summary>Vom WAN-Schritt gewählte Internet-Schnittstelle, damit der LAN-Schritt sie ausschließen kann.</summary>
    public string? WanInterfaceName { get; set; }

    public IReadOnlyList<ConfigurationAction>? WanActions { get; set; }

    public IReadOnlyList<ConfigurationAction>? LanActions { get; set; }

    public async Task DisposeClientAsync()
    {
        if (Client is not null)
        {
            await Client.DisposeAsync();
            Client = null;
        }
    }
}
