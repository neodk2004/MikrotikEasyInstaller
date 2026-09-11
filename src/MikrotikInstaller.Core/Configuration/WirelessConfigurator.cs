namespace MikrotikInstaller.Core.Configuration;

/// <summary>Erzeugt aus SSID/Passwort ein WPA2-Sicherheitsprofil und aktiviert es auf der WLAN-Schnittstelle.</summary>
public static class WirelessConfigurator
{
    public static IReadOnlyList<ConfigurationAction> BuildActions(WirelessSettings settings)
    {
        var profileName = $"profile-{settings.InterfaceName}";

        return
        [
            new ConfigurationAction(
                $"WLAN-Sicherheitsprofil für „{settings.Ssid}\" anlegen (WPA2)",
                (client, ct) => client.AddAsync("/interface/wireless/security-profiles", new Dictionary<string, string>
                {
                    ["name"] = profileName,
                    ["mode"] = "dynamic-keys",
                    ["authentication-types"] = "wpa2-psk",
                    ["wpa2-pre-shared-key"] = settings.Password,
                }, ct)),

            // "numbers" ist die RouterOS-API-Konvention, um einen Eintrag anhand seines Namens statt
            // seiner internen .id zu adressieren — vermeidet einen zusätzlichen Nachschlage-Aufruf.
            new ConfigurationAction(
                $"WLAN „{settings.Ssid}\" auf „{settings.InterfaceName}\" aktivieren",
                (client, ct) => client.ExecuteAsync("/interface/wireless/set", new Dictionary<string, string>
                {
                    ["numbers"] = settings.InterfaceName,
                    ["ssid"] = settings.Ssid,
                    ["security-profile"] = profileName,
                    ["disabled"] = "no",
                }, ct)),
        ];
    }

    /// <summary>Kurze, laientaugliche Zusammenfassung für den Zusammenfassungs-Schritt (keine RouterOS-Fachbegriffe).</summary>
    public static IReadOnlyList<string> BuildFriendlySummary(WirelessSettings settings) =>
        [$"WLAN „{settings.Ssid}\" ist eingerichtet und mit einem Passwort geschützt."];
}
