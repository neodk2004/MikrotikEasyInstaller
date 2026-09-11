using MikrotikInstaller.Core.Updates;

namespace MikrotikInstaller.Core.Tests.Updates;

public class RouterOsUpdateServiceTests
{
    [Fact]
    public async Task CheckForUpdatesAsync_UpdateAvailable_ReturnsIsUpdateAvailableTrue()
    {
        var responses = new Queue<IReadOnlyList<IReadOnlyDictionary<string, string>>>();
        responses.Enqueue([new Dictionary<string, string> { ["status"] = "New version is available", ["installed-version"] = "7.10", ["latest-version"] = "7.15.2" }]);
        var client = new SequencedRouterOsClient(responses);

        var info = await RouterOsUpdateService.CheckForUpdatesAsync(client);

        Assert.True(info.IsCheckSuccessful);
        Assert.True(info.IsUpdateAvailable);
        Assert.Equal("7.10", info.InstalledVersion);
        Assert.Equal("7.15.2", info.LatestVersion);
        Assert.Equal(1, client.ExecuteCallCount);
    }

    [Fact]
    public async Task CheckForUpdatesAsync_AlreadyUpToDate_ReturnsIsUpdateAvailableFalse()
    {
        var responses = new Queue<IReadOnlyList<IReadOnlyDictionary<string, string>>>();
        responses.Enqueue([new Dictionary<string, string> { ["status"] = "System is already up to date", ["installed-version"] = "7.15.2", ["latest-version"] = "7.15.2" }]);
        var client = new SequencedRouterOsClient(responses);

        var info = await RouterOsUpdateService.CheckForUpdatesAsync(client);

        Assert.True(info.IsCheckSuccessful);
        Assert.False(info.IsUpdateAvailable);
    }

    [Fact]
    public async Task CheckForUpdatesAsync_WaitsWhileStatusIsStillChecking()
    {
        var responses = new Queue<IReadOnlyList<IReadOnlyDictionary<string, string>>>();
        responses.Enqueue([new Dictionary<string, string> { ["status"] = "Checking for updates..." }]);
        responses.Enqueue([new Dictionary<string, string> { ["status"] = "Checking for updates..." }]);
        responses.Enqueue([new Dictionary<string, string> { ["status"] = "System is already up to date", ["installed-version"] = "7.15.2", ["latest-version"] = "7.15.2" }]);
        var client = new SequencedRouterOsClient(responses);

        var info = await RouterOsUpdateService.CheckForUpdatesAsync(client);

        Assert.True(info.IsCheckSuccessful);
        Assert.Empty(responses);
    }

    [Fact]
    public async Task CheckForUpdatesAsync_NoInternet_ReturnsCheckFailed()
    {
        // Kein Eintrag jemals verfügbar (Gerät kann den Update-Server nicht erreichen).
        var client = new SequencedRouterOsClient(new Queue<IReadOnlyList<IReadOnlyDictionary<string, string>>>());

        var info = await RouterOsUpdateService.CheckForUpdatesAsync(client);

        Assert.False(info.IsCheckSuccessful);
        Assert.False(info.IsUpdateAvailable);
    }
}
