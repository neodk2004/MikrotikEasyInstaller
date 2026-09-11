using MikrotikInstaller.Core.Configuration;

namespace MikrotikInstaller.Core.Tests.Configuration;

public class WanConfiguratorTests
{
    [Fact]
    public void BuildActions_DhcpMode_ProducesDhcpClientAndNat()
    {
        var actions = WanConfigurator.BuildActions(new WanSettings("ether1", WanAddressMode.Dhcp));

        Assert.Equal(2, actions.Count);
        Assert.Contains(actions, a => a.Description.Contains("DHCP") && a.Description.Contains("ether1"));
        Assert.Contains(actions, a => a.Description.Contains("NAT"));
    }

    [Fact]
    public void BuildActions_StaticMode_ProducesAddressRouteAndNat()
    {
        var actions = WanConfigurator.BuildActions(new WanSettings(
            "ether1", WanAddressMode.Static, StaticAddressCidr: "203.0.113.5/24", StaticGateway: "203.0.113.1"));

        Assert.Equal(3, actions.Count);
        Assert.Contains(actions, a => a.Description.Contains("203.0.113.5/24"));
        Assert.Contains(actions, a => a.Description.Contains("203.0.113.1"));
        Assert.Contains(actions, a => a.Description.Contains("NAT"));
    }

    [Fact]
    public async Task BuildActions_StaticMode_AppliesExpectedRouterOsCommands()
    {
        var actions = WanConfigurator.BuildActions(new WanSettings(
            "ether1", WanAddressMode.Static, StaticAddressCidr: "203.0.113.5/24", StaticGateway: "203.0.113.1"));
        var client = new RecordingRouterOsClient();

        foreach (var action in actions)
        {
            await action.ApplyAsync(client, CancellationToken.None);
        }

        Assert.Contains(client.AddCalls, c => c.Path == "/ip/address"
            && c.Parameters["address"] == "203.0.113.5/24" && c.Parameters["interface"] == "ether1");
        Assert.Contains(client.AddCalls, c => c.Path == "/ip/route"
            && c.Parameters["gateway"] == "203.0.113.1" && c.Parameters["dst-address"] == "0.0.0.0/0");
        Assert.Contains(client.AddCalls, c => c.Path == "/ip/firewall/nat"
            && c.Parameters["action"] == "masquerade" && c.Parameters["out-interface"] == "ether1");
    }

    [Fact]
    public void BuildFriendlySummary_ContainsNoRouterOsJargon()
    {
        var summary = WanConfigurator.BuildFriendlySummary(new WanSettings("ether1", WanAddressMode.Dhcp));

        Assert.All(summary, line => Assert.DoesNotContain("NAT", line));
        Assert.All(summary, line => Assert.DoesNotContain("DHCP-Client", line));
        Assert.Contains(summary, line => line.Contains("ether1"));
    }
}
