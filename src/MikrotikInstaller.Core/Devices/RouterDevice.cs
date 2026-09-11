namespace MikrotikInstaller.Core.Devices;

/// <summary>
/// Ergebnis der Geräte-Erkennung: alles, was die folgenden Wizard-Schritte über das Gerät wissen müssen.
/// </summary>
public sealed record RouterDevice(
    string IdentityName,
    string BoardModel,
    string RouterOsVersion,
    string Architecture,
    bool HasWireless,
    IReadOnlyList<InterfaceInfo> Interfaces);
