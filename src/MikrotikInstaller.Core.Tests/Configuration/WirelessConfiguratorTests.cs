using MikrotikInstaller.Core.Configuration;

namespace MikrotikInstaller.Core.Tests.Configuration;

public class WirelessConfiguratorTests
{
    [Fact]
    public async Task BuildActions_CreatesSecurityProfileAndActivatesInterfaceByName()
    {
        var actions = WirelessConfigurator.BuildActions(new WirelessSettings("wlan1", "Mein-WLAN", "sicheres-passwort"));
        var client = new RecordingRouterOsClient();

        foreach (var action in actions)
        {
            await action.ApplyAsync(client, CancellationToken.None);
        }

        Assert.Contains(client.AddCalls, c => c.Path == "/interface/wireless/security-profiles"
            && c.Parameters["wpa2-pre-shared-key"] == "sicheres-passwort" && c.Parameters["authentication-types"] == "wpa2-psk");
        Assert.Contains(client.ExecuteCalls, c => c.Path == "/interface/wireless/set"
            && c.Parameters!["numbers"] == "wlan1" && c.Parameters["ssid"] == "Mein-WLAN" && c.Parameters["disabled"] == "no");
    }

    [Fact]
    public void BuildFriendlySummary_MentionsSsidWithoutExposingPassword()
    {
        var summary = WirelessConfigurator.BuildFriendlySummary(new WirelessSettings("wlan1", "Mein-WLAN", "sicheres-passwort"));

        Assert.Contains(summary, line => line.Contains("Mein-WLAN"));
        Assert.All(summary, line => Assert.DoesNotContain("sicheres-passwort", line));
    }
}
