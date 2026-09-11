namespace MikrotikInstaller.Core.Configuration;

/// <summary>
/// Erzeugt aus den Wizard-Eingaben zum Heimnetzwerk die anzuwendenden RouterOS-Änderungen:
/// Bridge mit den gewählten Schnittstellen, IP-Adresse, DHCP-Server und DNS-Weiterleitung.
/// </summary>
public static class LanConfigurator
{
    public static IReadOnlyList<ConfigurationAction> BuildActions(LanSettings settings)
    {
        var actions = new List<ConfigurationAction>();
        var routerAddress = settings.RouterAddressCidr.Split('/')[0];
        var networkCidr = IpNetworkHelper.GetNetworkCidr(settings.RouterAddressCidr);
        var poolName = $"pool-{settings.BridgeName}";
        var dhcpServerName = $"dhcp-{settings.BridgeName}";

        actions.Add(new ConfigurationAction(
            $"Netzwerk-Bridge „{settings.BridgeName}\" für dein Heimnetzwerk anlegen",
            (client, ct) => client.AddAsync("/interface/bridge", new Dictionary<string, string>
            {
                ["name"] = settings.BridgeName,
            }, ct)));

        foreach (var interfaceName in settings.MemberInterfaces)
        {
            actions.Add(new ConfigurationAction(
                $"Schnittstelle „{interfaceName}\" dem Heimnetzwerk hinzufügen",
                (client, ct) => client.AddAsync("/interface/bridge/port", new Dictionary<string, string>
                {
                    ["bridge"] = settings.BridgeName,
                    ["interface"] = interfaceName,
                }, ct)));
        }

        actions.Add(new ConfigurationAction(
            $"IP-Adresse {settings.RouterAddressCidr} auf „{settings.BridgeName}\" einrichten",
            (client, ct) => client.AddAsync("/ip/address", new Dictionary<string, string>
            {
                ["address"] = settings.RouterAddressCidr,
                ["interface"] = settings.BridgeName,
            }, ct)));

        actions.Add(new ConfigurationAction(
            $"IP-Adressbereich {settings.DhcpPoolStart}-{settings.DhcpPoolEnd} für Geräte in deinem Netzwerk reservieren",
            (client, ct) => client.AddAsync("/ip/pool", new Dictionary<string, string>
            {
                ["name"] = poolName,
                ["ranges"] = $"{settings.DhcpPoolStart}-{settings.DhcpPoolEnd}",
            }, ct)));

        actions.Add(new ConfigurationAction(
            $"DHCP-Server auf „{settings.BridgeName}\" aktivieren, damit Geräte automatisch eine Adresse bekommen",
            (client, ct) => client.AddAsync("/ip/dhcp-server", new Dictionary<string, string>
            {
                ["name"] = dhcpServerName,
                ["interface"] = settings.BridgeName,
                ["address-pool"] = poolName,
                ["disabled"] = "no",
            }, ct)));

        actions.Add(new ConfigurationAction(
            $"Netzwerk-Einstellungen (Gateway {routerAddress}, DNS) für das Heimnetzwerk hinterlegen",
            (client, ct) => client.AddAsync("/ip/dhcp-server/network", new Dictionary<string, string>
            {
                ["address"] = networkCidr,
                ["gateway"] = routerAddress,
                ["dns-server"] = routerAddress,
            }, ct)));

        var upstreamDns = settings.SecondaryDnsServer is null
            ? settings.PrimaryDnsServer
            : $"{settings.PrimaryDnsServer},{settings.SecondaryDnsServer}";

        actions.Add(new ConfigurationAction(
            $"Router als DNS-Server für dein Heimnetz einrichten (leitet an {upstreamDns} weiter)",
            (client, ct) => client.ExecuteAsync("/ip/dns/set", new Dictionary<string, string>
            {
                ["servers"] = upstreamDns,
                ["allow-remote-requests"] = "yes",
            }, ct)));

        return actions;
    }

    /// <summary>Kurze, laientaugliche Zusammenfassung für den Zusammenfassungs-Schritt (keine RouterOS-Fachbegriffe).</summary>
    public static IReadOnlyList<string> BuildFriendlySummary(LanSettings settings)
    {
        var networkCidr = IpNetworkHelper.GetNetworkCidr(settings.RouterAddressCidr);
        var members = string.Join(", ", settings.MemberInterfaces.Select(name => $"„{name}\""));

        return
        [
            $"Geräte in deinem Heimnetz ({networkCidr}) bekommen automatisch eine Adresse.",
            $"Angeschlossen sind: {members}.",
        ];
    }
}
