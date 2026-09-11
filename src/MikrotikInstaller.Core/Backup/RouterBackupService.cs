using MikrotikInstaller.Core.Connectivity;

namespace MikrotikInstaller.Core.Backup;

/// <summary>
/// Erstellt vor jeder Änderung ein Backup auf dem Gerät selbst (RouterOS "/system/backup/save").
/// Das Backup liegt danach als Datei im Dateispeicher des Geräts (abrufbar z. B. über WinBox/Files).
/// </summary>
public static class RouterBackupService
{
    public static async Task<string> CreateBackupAsync(IRouterOsClient client, CancellationToken cancellationToken = default)
    {
        var backupName = $"mikrotik-installer-{DateTime.Now:yyyyMMdd-HHmmss}";

        await client.ExecuteAsync("/system/backup/save", new Dictionary<string, string>
        {
            ["name"] = backupName,
        }, cancellationToken);

        return backupName;
    }
}
