using MikrotikInstaller.Core.Connectivity;
using MikrotikInstaller.Core.Connectivity.Demo;

namespace MikrotikInstaller.Core.Tests.Connectivity;

public class DemoRouterOsClientTests
{
    [Fact]
    public void Matches_OnlyExactTestCredentials()
    {
        Assert.True(DemoRouterOsClient.Matches(new RouterOsCredentials("127.127.127.127", "admin", "admin")));
        Assert.False(DemoRouterOsClient.Matches(new RouterOsCredentials("192.168.88.1", "admin", "admin")));
        Assert.False(DemoRouterOsClient.Matches(new RouterOsCredentials("127.127.127.127", "admin", "falsch")));
    }

    [Fact]
    public async Task GetAsync_ReturnsSimulatedInterfacesAndResource()
    {
        var client = new DemoRouterOsClient();

        var interfaces = await client.GetAsync("/interface");
        var resource = await client.GetAsync("/system/resource");

        Assert.NotEmpty(interfaces);
        Assert.Contains(interfaces, i => i["name"] == "ether1");
        Assert.Single(resource);
    }

    [Fact]
    public async Task AddAsync_StoresRowAndReturnsId()
    {
        var client = new DemoRouterOsClient();

        var id = await client.AddAsync("/interface/bridge", new Dictionary<string, string> { ["name"] = "bridge-lan" });
        var rows = await client.GetAsync("/interface/bridge");

        Assert.False(string.IsNullOrEmpty(id));
        Assert.Contains(rows, r => r["name"] == "bridge-lan" && r[".id"] == id);
    }
}
