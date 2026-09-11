namespace MikrotikInstaller.Core.Configuration;

public enum WanAddressMode
{
    /// <summary>Adresse automatisch vom Internetanbieter per DHCP beziehen (häufigster Fall).</summary>
    Dhcp,

    /// <summary>Feste, vom Nutzer vorgegebene IP-Adresse verwenden.</summary>
    Static,
}

public sealed record WanSettings(
    string InterfaceName,
    WanAddressMode Mode,
    string? StaticAddressCidr = null,
    string? StaticGateway = null);
