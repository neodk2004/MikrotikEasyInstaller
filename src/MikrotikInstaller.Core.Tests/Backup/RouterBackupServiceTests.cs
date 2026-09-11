using MikrotikInstaller.Core.Backup;
using MikrotikInstaller.Core.Tests.Configuration;

namespace MikrotikInstaller.Core.Tests.Backup;

public class RouterBackupServiceTests
{
    [Fact]
    public async Task CreateBackupAsync_ExecutesBackupSaveWithGeneratedName()
    {
        var client = new RecordingRouterOsClient();

        var backupName = await RouterBackupService.CreateBackupAsync(client);

        Assert.False(string.IsNullOrWhiteSpace(backupName));
        Assert.Contains(client.ExecuteCalls, c => c.Path == "/system/backup/save" && c.Parameters!["name"] == backupName);
    }
}
