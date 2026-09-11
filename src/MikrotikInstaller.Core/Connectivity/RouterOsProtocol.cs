namespace MikrotikInstaller.Core.Connectivity;

/// <summary>
/// Über welches Transportprotokoll die Verbindung zum RouterOS-Gerät tatsächlich läuft.
/// Rein informativ für Diagnose/Anzeige — die Konfigurationslogik arbeitet ausschließlich gegen <see cref="IRouterOsClient"/>.
/// </summary>
public enum RouterOsProtocol
{
    /// <summary>RouterOS 7+ REST-API (HTTP/HTTPS, JSON).</summary>
    Rest,

    /// <summary>Binäre RouterOS-API (Port 8728/8729) — Fallback für ältere Geräte.</summary>
    Binary,
}
