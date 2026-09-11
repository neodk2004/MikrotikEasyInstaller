using MikrotikInstaller.Core.Configuration;

namespace MikrotikInstaller.Core.Tests.Configuration;

public class FirewallConfiguratorTests
{
    [Fact]
    public async Task BuildActions_WithoutPortForwards_BlocksNewWanInputAndForward()
    {
        var settings = new FirewallSettings("ether1", "bridge-lan", []);
        var actions = FirewallConfigurator.BuildActions(settings);
        var client = new RecordingRouterOsClient();

        foreach (var action in actions)
        {
            await action.ApplyAsync(client, CancellationToken.None);
        }

        var filterCalls = client.AddCalls.Where(c => c.Path == "/ip/firewall/filter").ToList();

        Assert.Contains(filterCalls, c => c.Parameters["chain"] == "input" && c.Parameters.GetValueOrDefault("in-interface") == "bridge-lan" && c.Parameters["action"] == "accept");
        Assert.Contains(filterCalls, c => c.Parameters["chain"] == "input" && c.Parameters.GetValueOrDefault("in-interface") == "ether1" && c.Parameters["action"] == "drop");
        Assert.Contains(filterCalls, c => c.Parameters["chain"] == "forward" && c.Parameters.GetValueOrDefault("in-interface") == "ether1" && c.Parameters["action"] == "drop");
        Assert.Empty(client.AddCalls.Where(c => c.Path == "/ip/firewall/nat"));
    }

    [Fact]
    public async Task BuildActions_PortForwardRulesComeBeforeFinalWanDropRule()
    {
        var settings = new FirewallSettings("ether1", "bridge-lan",
            [new PortForward("Spieleserver", "udp", 25565, "192.168.88.50", 25565)]);
        var actions = FirewallConfigurator.BuildActions(settings);
        var client = new RecordingRouterOsClient();

        foreach (var action in actions)
        {
            await action.ApplyAsync(client, CancellationToken.None);
        }

        Assert.Contains(client.AddCalls, c => c.Path == "/ip/firewall/nat"
            && c.Parameters["chain"] == "dstnat" && c.Parameters["to-addresses"] == "192.168.88.50" && c.Parameters["to-ports"] == "25565");

        var forwardChainCalls = client.AddCalls.Where(c => c.Path == "/ip/firewall/filter" && c.Parameters["chain"] == "forward").ToList();
        var allowIndex = forwardChainCalls.FindIndex(c => c.Parameters.GetValueOrDefault("dst-address") == "192.168.88.50");
        var dropIndex = forwardChainCalls.FindIndex(c => c.Parameters["action"] == "drop" && c.Parameters.ContainsKey("in-interface"));

        Assert.True(allowIndex >= 0 && dropIndex >= 0 && allowIndex < dropIndex);
    }

    [Fact]
    public void BuildFriendlySummary_WithoutPortForwards_OnlyMentionsProtection()
    {
        var summary = FirewallConfigurator.BuildFriendlySummary(new FirewallSettings("ether1", "bridge-lan", []));

        Assert.Single(summary);
    }

    [Fact]
    public void BuildFriendlySummary_WithPortForwards_MentionsThem()
    {
        var settings = new FirewallSettings("ether1", "bridge-lan",
            [new PortForward("Spieleserver", "udp", 25565, "192.168.88.50", 25565)]);

        var summary = FirewallConfigurator.BuildFriendlySummary(settings);

        Assert.Contains(summary, line => line.Contains("Spieleserver") && line.Contains("25565"));
    }
}
