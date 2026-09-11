namespace MikrotikInstaller.Core.Configuration;

/// <summary>Erzeugt aus den Wizard-Eingaben zum Internet-Zugang die anzuwendenden RouterOS-Änderungen.</summary>
public static class WanConfigurator
{
    public static IReadOnlyList<ConfigurationAction> BuildActions(WanSettings settings)
    {
        var actions = new List<ConfigurationAction>();

        if (settings.Mode == WanAddressMode.Dhcp)
        {
            actions.Add(new ConfigurationAction(
                $"Internetadresse automatisch über DHCP auf „{settings.InterfaceName}\" beziehen",
                (client, ct) => client.AddAsync("/ip/dhcp-client", new Dictionary<string, string>
                {
                    ["interface"] = settings.InterfaceName,
                    ["disabled"] = "no",
                }, ct)));
        }
        else
        {
            actions.Add(new ConfigurationAction(
                $"Feste IP-Adresse {settings.StaticAddressCidr} auf „{settings.InterfaceName}\" einrichten",
                (client, ct) => client.AddAsync("/ip/address", new Dictionary<string, string>
                {
                    ["address"] = settings.StaticAddressCidr!,
                    ["interface"] = settings.InterfaceName,
                }, ct)));

            actions.Add(new ConfigurationAction(
                $"Standard-Internetroute über {settings.StaticGateway} eintragen",
                (client, ct) => client.AddAsync("/ip/route", new Dictionary<string, string>
                {
                    ["dst-address"] = "0.0.0.0/0",
                    ["gateway"] = settings.StaticGateway!,
                }, ct)));
        }

        actions.Add(new ConfigurationAction(
            $"Datenverkehr aus deinem Heimnetz über „{settings.InterfaceName}\" ins Internet weiterleiten (NAT)",
            (client, ct) => client.AddAsync("/ip/firewall/nat", new Dictionary<string, string>
            {
                ["chain"] = "srcnat",
                ["out-interface"] = settings.InterfaceName,
                ["action"] = "masquerade",
            }, ct)));

        return actions;
    }
}
