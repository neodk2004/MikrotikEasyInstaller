using MikrotikInstaller.Core.Connectivity;
using MikrotikInstaller.Core.Connectivity.Demo;

namespace MikrotikInstaller.Core.Tests.Connectivity;

public class RouterOsClientFactoryTests
{
    [Fact]
    public async Task ConnectAsync_WithDemoCredentials_ReturnsDemoClientImmediately()
    {
        var credentials = new RouterOsCredentials(DemoRouterOsClient.DemoHost, DemoRouterOsClient.DemoUsername, DemoRouterOsClient.DemoPassword);

        await using var client = await RouterOsClientFactory.ConnectAsync(credentials).WaitAsync(TimeSpan.FromSeconds(5));

        Assert.IsType<DemoRouterOsClient>(client);
    }
}
