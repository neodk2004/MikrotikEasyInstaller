using MikrotikInstaller.Core.Connectivity;

namespace MikrotikInstaller.Core.Devices;

/// <summary>
/// Liest die für den Assistenten relevanten Eckdaten eines RouterOS-Geräts aus.
/// Die Anfragen laufen bewusst nacheinander statt parallel: Die binäre API verarbeitet auf
/// einer einzelnen TCP-Verbindung immer nur einen Befehl gleichzeitig.
/// </summary>
public static class DeviceDiscoveryService
{
    private static readonly IReadOnlyDictionary<string, string> EmptyRow = new Dictionary<string, string>();

    public static async Task<RouterDevice> DiscoverAsync(IRouterOsClient client, CancellationToken cancellationToken = default)
    {
        var resource = (await client.GetAsync("/system/resource", cancellationToken)).FirstOrDefault() ?? EmptyRow;
        var identity = (await client.GetAsync("/system/identity", cancellationToken)).FirstOrDefault() ?? EmptyRow;
        var routerboard = (await client.GetAsync("/system/routerboard", cancellationToken)).FirstOrDefault() ?? EmptyRow;
        var interfaceRows = await client.GetAsync("/interface", cancellationToken);

        var interfaces = interfaceRows.Select(ToInterfaceInfo).ToList();

        var boardModel = routerboard.GetValueOrDefault("model");
        if (string.IsNullOrWhiteSpace(boardModel))
        {
            boardModel = resource.GetValueOrDefault("board-name", "Unbekanntes Modell");
        }

        return new RouterDevice(
            IdentityName: identity.GetValueOrDefault("name", "MikroTik"),
            BoardModel: boardModel,
            RouterOsVersion: resource.GetValueOrDefault("version", "unbekannt"),
            Architecture: resource.GetValueOrDefault("architecture-name", "unbekannt"),
            HasWireless: interfaces.Any(i => i.Kind == InterfaceKind.Wireless),
            Interfaces: interfaces);
    }

    private static InterfaceInfo ToInterfaceInfo(IReadOnlyDictionary<string, string> row)
    {
        var kind = row.GetValueOrDefault("type", string.Empty) switch
        {
            "ether" => InterfaceKind.Ethernet,
            "bridge" => InterfaceKind.Bridge,
            "vlan" => InterfaceKind.Vlan,
            "wlan" or "wifi" or "wifiwave2" => InterfaceKind.Wireless,
            _ => InterfaceKind.Other,
        };

        return new InterfaceInfo(
            Name: row.GetValueOrDefault("name", "?"),
            Kind: kind,
            IsRunning: row.GetValueOrDefault("running") == "true",
            IsDisabled: row.GetValueOrDefault("disabled") == "true",
            Comment: row.GetValueOrDefault("comment"));
    }
}
