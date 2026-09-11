namespace MikrotikInstaller.Core.Updates;

/// <summary>
/// Ergebnis einer Update-Prüfung. <see cref="IsCheckSuccessful"/> ist <c>false</c>, wenn das Gerät
/// den MikroTik-Update-Server nicht erreichen konnte (z. B. weil noch kein Internetzugang eingerichtet
/// ist) — das ist beim allerersten Einrichtungsschritt der Normalfall, kein Fehler.
/// </summary>
public sealed record RouterOsUpdateInfo(
    bool IsCheckSuccessful,
    string InstalledVersion,
    string? LatestVersion,
    bool IsUpdateAvailable)
{
    public static RouterOsUpdateInfo CheckFailed() => new(false, string.Empty, null, false);
}
