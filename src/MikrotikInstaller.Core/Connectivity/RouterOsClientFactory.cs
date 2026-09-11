using MikrotikInstaller.Core.Connectivity.Binary;
using MikrotikInstaller.Core.Connectivity.Rest;

namespace MikrotikInstaller.Core.Connectivity;

/// <summary>
/// Baut die Verbindung zu einem RouterOS-Gerät auf: versucht zuerst die REST-API (RouterOS 7+),
/// fällt bei Fehlschlag automatisch auf die binäre API zurück (ältere Geräte). Das UI erfährt nur
/// Erfolg/Misserfolg mit laientauglicher Meldung — nie, welches Protokoll intern versucht wurde.
/// </summary>
public static class RouterOsClientFactory
{
    public static async Task<IRouterOsClient> ConnectAsync(RouterOsCredentials credentials, CancellationToken cancellationToken = default)
    {
        try
        {
            return await RestRouterOsClient.ConnectAsync(credentials, cancellationToken);
        }
        catch (RouterOsException)
        {
            // REST nicht verfügbar (z. B. älteres RouterOS oder REST-Dienst deaktiviert) — auf Binary-API ausweichen.
        }

        try
        {
            return await BinaryRouterOsClient.ConnectAsync(credentials, cancellationToken);
        }
        catch (RouterOsException)
        {
            throw new RouterOsException(
                "Es konnte keine Verbindung zum Gerät hergestellt werden. Bitte überprüfe Adresse, Benutzername und Passwort.");
        }
    }
}
