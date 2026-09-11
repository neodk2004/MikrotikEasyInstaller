using MikrotikInstaller.Core.Configuration;

namespace MikrotikInstaller.Core.Tests.Configuration;

public class LanConfiguratorTests
{
    private static LanSettings DefaultSettings() => new(
        BridgeName: "bridge-lan",
        MemberInterfaces: ["ether2", "ether3"],
        RouterAddressCidr: "192.168.88.1/24",
        DhcpPoolStart: "192.168.88.10",
        DhcpPoolEnd: "192.168.88.254",
        PrimaryDnsServer: "1.1.1.1",
        SecondaryDnsServer: "8.8.8.8");

    [Fact]
    public void BuildActions_OneActionPerBridgePortPlusFixedSteps()
    {
        var actions = LanConfigurator.BuildActions(DefaultSettings());

        // Bridge anlegen + 2 Ports + IP-Adresse + Pool + DHCP-Server + Netzwerk + DNS = 8
        Assert.Equal(8, actions.Count);
    }

    [Fact]
    public async Task BuildActions_AppliesExpectedRouterOsCommands()
    {
        var actions = LanConfigurator.BuildActions(DefaultSettings());
        var client = new RecordingRouterOsClient();

        foreach (var action in actions)
        {
            await action.ApplyAsync(client, CancellationToken.None);
        }

        Assert.Contains(client.AddCalls, c => c.Path == "/interface/bridge" && c.Parameters["name"] == "bridge-lan");
        Assert.Contains(client.AddCalls, c => c.Path == "/interface/bridge/port"
            && c.Parameters["bridge"] == "bridge-lan" && c.Parameters["interface"] == "ether2");
        Assert.Contains(client.AddCalls, c => c.Path == "/interface/bridge/port"
            && c.Parameters["bridge"] == "bridge-lan" && c.Parameters["interface"] == "ether3");
        Assert.Contains(client.AddCalls, c => c.Path == "/ip/address" && c.Parameters["address"] == "192.168.88.1/24");
        Assert.Contains(client.AddCalls, c => c.Path == "/ip/pool" && c.Parameters["ranges"] == "192.168.88.10-192.168.88.254");
        Assert.Contains(client.AddCalls, c => c.Path == "/ip/dhcp-server" && c.Parameters["interface"] == "bridge-lan");
        Assert.Contains(client.AddCalls, c => c.Path == "/ip/dhcp-server/network"
            && c.Parameters["address"] == "192.168.88.0/24" && c.Parameters["gateway"] == "192.168.88.1");
        Assert.Contains(client.ExecuteCalls, c => c.Path == "/ip/dns/set"
            && c.Parameters!["servers"] == "1.1.1.1,8.8.8.8" && c.Parameters["allow-remote-requests"] == "yes");
    }
}
