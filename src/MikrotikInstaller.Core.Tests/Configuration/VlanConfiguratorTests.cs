using MikrotikInstaller.Core.Configuration;

namespace MikrotikInstaller.Core.Tests.Configuration;

public class VlanConfiguratorTests
{
    [Fact]
    public async Task BuildActions_CreatesVlanInterfaceAddressPoolAndDhcp()
    {
        var vlans = new List<VlanDefinition>
        {
            new(VlanId: 20, Name: "Gäste", RouterAddressCidr: "192.168.20.1/24", DhcpPoolStart: "192.168.20.10", DhcpPoolEnd: "192.168.20.254"),
        };
        var actions = VlanConfigurator.BuildActions("bridge-lan", vlans);
        var client = new RecordingRouterOsClient();

        foreach (var action in actions)
        {
            await action.ApplyAsync(client, CancellationToken.None);
        }

        Assert.Contains(client.AddCalls, c => c.Path == "/interface/vlan"
            && c.Parameters["vlan-id"] == "20" && c.Parameters["interface"] == "bridge-lan" && c.Parameters["name"] == "vlan20");
        Assert.Contains(client.AddCalls, c => c.Path == "/ip/address" && c.Parameters["address"] == "192.168.20.1/24" && c.Parameters["interface"] == "vlan20");
        Assert.Contains(client.AddCalls, c => c.Path == "/ip/pool" && c.Parameters["ranges"] == "192.168.20.10-192.168.20.254");
        Assert.Contains(client.AddCalls, c => c.Path == "/ip/dhcp-server" && c.Parameters["interface"] == "vlan20");
        Assert.Contains(client.AddCalls, c => c.Path == "/ip/dhcp-server/network" && c.Parameters["address"] == "192.168.20.0/24");
    }

    [Fact]
    public void BuildActions_NoVlans_ReturnsEmptyList()
    {
        var actions = VlanConfigurator.BuildActions("bridge-lan", []);
        Assert.Empty(actions);
    }

    [Fact]
    public void BuildFriendlySummary_OneLinePerVlan()
    {
        var vlans = new List<VlanDefinition>
        {
            new(20, "Gäste", "192.168.20.1/24", "192.168.20.10", "192.168.20.254"),
            new(30, "IoT", "192.168.30.1/24", "192.168.30.10", "192.168.30.254"),
        };

        var summary = VlanConfigurator.BuildFriendlySummary(vlans);

        Assert.Equal(2, summary.Count);
        Assert.Contains(summary, line => line.Contains("Gäste"));
        Assert.Contains(summary, line => line.Contains("IoT"));
    }
}
