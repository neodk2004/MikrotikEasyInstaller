using MikrotikInstaller.Core.Devices;

namespace MikrotikInstaller.Core.Tests.Devices;

public class DeviceDiscoveryServiceTests
{
    [Fact]
    public async Task DiscoverAsync_MapsResourceIdentityAndInterfaces()
    {
        var client = new FakeRouterOsClient(new Dictionary<string, IReadOnlyList<IReadOnlyDictionary<string, string>>>
        {
            ["/system/resource"] = [new Dictionary<string, string> { ["board-name"] = "CHR", ["version"] = "7.15.2 (stable)", ["architecture-name"] = "x86_64" }],
            ["/system/identity"] = [new Dictionary<string, string> { ["name"] = "MeinRouter" }],
            ["/system/routerboard"] = [new Dictionary<string, string> { ["routerboard"] = "no" }],
            ["/interface"] =
            [
                new Dictionary<string, string> { ["name"] = "ether1", ["type"] = "ether", ["running"] = "true", ["disabled"] = "false" },
                new Dictionary<string, string> { ["name"] = "wlan1", ["type"] = "wlan", ["running"] = "false", ["disabled"] = "true" },
            ],
        });

        var device = await DeviceDiscoveryService.DiscoverAsync(client);

        Assert.Equal("MeinRouter", device.IdentityName);
        Assert.Equal("CHR", device.BoardModel);
        Assert.Equal("7.15.2 (stable)", device.RouterOsVersion);
        Assert.Equal("x86_64", device.Architecture);
        Assert.True(device.HasWireless);
        Assert.Equal(2, device.Interfaces.Count);
        Assert.Equal(InterfaceKind.Ethernet, device.Interfaces[0].Kind);
        Assert.True(device.Interfaces[0].IsRunning);
        Assert.Equal(InterfaceKind.Wireless, device.Interfaces[1].Kind);
        Assert.True(device.Interfaces[1].IsDisabled);
    }

    [Fact]
    public async Task DiscoverAsync_PrefersRouterboardModelOverBoardName()
    {
        var client = new FakeRouterOsClient(new Dictionary<string, IReadOnlyList<IReadOnlyDictionary<string, string>>>
        {
            ["/system/resource"] = [new Dictionary<string, string> { ["board-name"] = "generic" }],
            ["/system/routerboard"] = [new Dictionary<string, string> { ["routerboard"] = "yes", ["model"] = "RB750Gr3" }],
        });

        var device = await DeviceDiscoveryService.DiscoverAsync(client);

        Assert.Equal("RB750Gr3", device.BoardModel);
    }

    [Fact]
    public async Task DiscoverAsync_NoWirelessInterfaces_HasWirelessIsFalse()
    {
        var client = new FakeRouterOsClient(new Dictionary<string, IReadOnlyList<IReadOnlyDictionary<string, string>>>
        {
            ["/interface"] =
            [
                new Dictionary<string, string> { ["name"] = "ether1", ["type"] = "ether" },
                new Dictionary<string, string> { ["name"] = "bridge1", ["type"] = "bridge" },
            ],
        });

        var device = await DeviceDiscoveryService.DiscoverAsync(client);

        Assert.False(device.HasWireless);
    }
}
