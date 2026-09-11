using MikrotikInstaller.Core.Configuration;

namespace MikrotikInstaller.Core.Export;

/// <summary>Alles, was auf das gedruckte Einrichtungsprotokoll (PDF) soll.</summary>
public sealed record SummaryDocumentData(
    string DeviceIdentityName,
    string DeviceModel,
    WanSettings? Wan,
    LanSettings? Lan,
    IReadOnlyList<VlanDefinition> Vlans,
    WirelessSettings? Wireless,
    FirewallSettings? Firewall,
    string? BackupName);
