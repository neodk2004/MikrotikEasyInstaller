using MikrotikInstaller.Core.Connectivity;
using MikrotikInstaller.Core.Devices;

namespace MikrotikInstaller.App.ViewModels;

/// <summary>
/// Über den gesamten Assistenten geteilter Zustand: die einmal hergestellte Geräteverbindung
/// und die daraus abgeleiteten Erkenntnisse, damit spätere Schritte nicht erneut verbinden müssen.
/// </summary>
public sealed class WizardSession
{
    public IRouterOsClient? Client { get; set; }

    public RouterDevice? Device { get; set; }

    public async Task DisposeClientAsync()
    {
        if (Client is not null)
        {
            await Client.DisposeAsync();
            Client = null;
        }
    }
}
