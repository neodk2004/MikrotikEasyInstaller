namespace MikrotikInstaller.Core.Connectivity;

/// <summary>
/// Zugangsdaten für ein MikroTik-Gerät. Wird sowohl von der REST- als auch der binären API verwendet.
/// </summary>
public sealed record RouterOsCredentials(string Host, string Username, string Password)
{
    /// <summary>Fester REST-Port. Wenn nicht gesetzt: 443 (verschlüsselt) bzw. 80 (unverschlüsselt).</summary>
    public int? RestPort { get; init; }

    /// <summary>Fester Port für die binäre API. Wenn nicht gesetzt: 8729 (verschlüsselt) bzw. 8728 (unverschlüsselt).</summary>
    public int? BinaryPort { get; init; }

    /// <summary>
    /// Ob verschlüsselt verbunden werden soll (HTTPS bzw. API-SSL). Auf einem MikroTik-Gerät ab Werk ist das
    /// standardmäßig nicht eingerichtet, daher ist der Standardwert hier bewusst <c>false</c>.
    /// </summary>
    public bool UseEncryption { get; init; }

    /// <summary>
    /// Ob ein selbstsigniertes/unbekanntes TLS-Zertifikat akzeptiert werden soll. Nur relevant, wenn
    /// <see cref="UseEncryption"/> aktiv ist. Muss im UI mit einem Warnhinweis verbunden sein — kein stiller Bypass.
    /// </summary>
    public bool AllowUntrustedCertificate { get; init; } = true;
}
