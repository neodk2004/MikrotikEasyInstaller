using MikrotikInstaller.Core.Connectivity;

namespace MikrotikInstaller.Core.Updates;

/// <summary>
/// Prüft über die RouterOS-eigene Update-Funktion, ob eine neuere RouterOS-Version verfügbar ist,
/// und kann eine gefundene Aktualisierung anstoßen. Das Gerät braucht dafür funktionierenden
/// Internetzugang — schlägt die Prüfung fehl, wird das als "nicht geprüft werden konnte" behandelt,
/// nicht als Fehler.
/// </summary>
public static class RouterOsUpdateService
{
    private const int PollAttempts = 12;
    private static readonly TimeSpan PollDelay = TimeSpan.FromMilliseconds(500);

    public static async Task<RouterOsUpdateInfo> CheckForUpdatesAsync(IRouterOsClient client, CancellationToken cancellationToken = default)
    {
        try
        {
            await client.ExecuteAsync("/system/package/update/check-for-updates", cancellationToken: cancellationToken);

            for (var attempt = 0; attempt < PollAttempts; attempt++)
            {
                await Task.Delay(PollDelay, cancellationToken);

                var rows = await client.GetAsync("/system/package/update", cancellationToken);
                var row = rows.FirstOrDefault();
                if (row is null)
                {
                    return RouterOsUpdateInfo.CheckFailed();
                }

                var status = row.GetValueOrDefault("status", string.Empty);
                if (status.Contains("checking", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var installed = row.GetValueOrDefault("installed-version", string.Empty);
                var latest = row.GetValueOrDefault("latest-version");
                var isAvailable = !string.IsNullOrWhiteSpace(latest)
                    && !string.Equals(latest, installed, StringComparison.OrdinalIgnoreCase);

                return new RouterOsUpdateInfo(true, installed, latest, isAvailable);
            }

            return RouterOsUpdateInfo.CheckFailed();
        }
        catch (RouterOsException)
        {
            return RouterOsUpdateInfo.CheckFailed();
        }
    }

    public static Task StartUpdateAsync(IRouterOsClient client, CancellationToken cancellationToken = default) =>
        client.ExecuteAsync("/system/package/update/install", cancellationToken: cancellationToken);
}
