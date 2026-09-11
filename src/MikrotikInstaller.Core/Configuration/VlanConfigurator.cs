namespace MikrotikInstaller.Core.Configuration;

/// <summary>
/// Erzeugt aus den Wizard-Eingaben zusätzliche, logisch getrennte Netzwerke: je VLAN eine eigene
/// VLAN-Schnittstelle auf der Heimnetz-Bridge mit eigenem Adressbereich und eigenem DHCP-Server.
/// </summary>
public static class VlanConfigurator
{
    public static IReadOnlyList<ConfigurationAction> BuildActions(string parentBridgeName, IReadOnlyList<VlanDefinition> vlans)
    {
        var actions = new List<ConfigurationAction>();

        foreach (var vlan in vlans)
        {
            var interfaceName = $"vlan{vlan.VlanId}";
            var routerAddress = vlan.RouterAddressCidr.Split('/')[0];
            var networkCidr = IpNetworkHelper.GetNetworkCidr(vlan.RouterAddressCidr);
            var poolName = $"pool-{interfaceName}";
            var dhcpServerName = $"dhcp-{interfaceName}";

            actions.Add(new ConfigurationAction(
                $"VLAN „{vlan.Name}\" (ID {vlan.VlanId}) auf „{parentBridgeName}\" anlegen",
                (client, ct) => client.AddAsync("/interface/vlan", new Dictionary<string, string>
                {
                    ["name"] = interfaceName,
                    ["interface"] = parentBridgeName,
                    ["vlan-id"] = vlan.VlanId.ToString(),
                }, ct)));

            actions.Add(new ConfigurationAction(
                $"IP-Adresse {vlan.RouterAddressCidr} für VLAN „{vlan.Name}\" einrichten",
                (client, ct) => client.AddAsync("/ip/address", new Dictionary<string, string>
                {
                    ["address"] = vlan.RouterAddressCidr,
                    ["interface"] = interfaceName,
                }, ct)));

            actions.Add(new ConfigurationAction(
                $"IP-Adressbereich {vlan.DhcpPoolStart}-{vlan.DhcpPoolEnd} für VLAN „{vlan.Name}\" reservieren",
                (client, ct) => client.AddAsync("/ip/pool", new Dictionary<string, string>
                {
                    ["name"] = poolName,
                    ["ranges"] = $"{vlan.DhcpPoolStart}-{vlan.DhcpPoolEnd}",
                }, ct)));

            actions.Add(new ConfigurationAction(
                $"DHCP-Server für VLAN „{vlan.Name}\" aktivieren",
                (client, ct) => client.AddAsync("/ip/dhcp-server", new Dictionary<string, string>
                {
                    ["name"] = dhcpServerName,
                    ["interface"] = interfaceName,
                    ["address-pool"] = poolName,
                    ["disabled"] = "no",
                }, ct)));

            actions.Add(new ConfigurationAction(
                $"Netzwerk-Einstellungen (Gateway {routerAddress}) für VLAN „{vlan.Name}\" hinterlegen",
                (client, ct) => client.AddAsync("/ip/dhcp-server/network", new Dictionary<string, string>
                {
                    ["address"] = networkCidr,
                    ["gateway"] = routerAddress,
                    ["dns-server"] = routerAddress,
                }, ct)));
        }

        return actions;
    }
}
