namespace MikrotikInstaller.Core.Configuration;

/// <summary>
/// Erzeugt eine sichere Basis-Firewall: Verwaltungszugriff auf den Router nur aus dem Heimnetz,
/// neue unangeforderte Verbindungen aus dem Internet werden blockiert — mit Ausnahme der vom
/// Nutzer ausdrücklich gewählten Portfreigaben.
/// </summary>
public static class FirewallConfigurator
{
    public static IReadOnlyList<ConfigurationAction> BuildActions(FirewallSettings settings)
    {
        var actions = new List<ConfigurationAction>
        {
            InputRule(
                "Bereits bestehende Verbindungen zum Router weiterhin erlauben",
                new Dictionary<string, string> { ["connection-state"] = "established,related,untracked", ["action"] = "accept" }),
            InputRule(
                "Ungültige Verbindungen zum Router verwerfen",
                new Dictionary<string, string> { ["connection-state"] = "invalid", ["action"] = "drop" }),
            InputRule(
                "Anpingen des Routers erlauben (zur Fehlersuche)",
                new Dictionary<string, string> { ["protocol"] = "icmp", ["action"] = "accept" }),
            InputRule(
                $"Zugriff auf die Router-Einstellungen aus deinem Heimnetz („{settings.LanBridgeName}\") erlauben",
                new Dictionary<string, string> { ["in-interface"] = settings.LanBridgeName, ["action"] = "accept" }),
        };

        // Portfreigaben müssen vor der abschließenden Sperrregel stehen, damit sie greifen.
        foreach (var forward in settings.PortForwards)
        {
            actions.Add(new ConfigurationAction(
                $"Port {forward.ExternalPort}/{forward.Protocol.ToUpperInvariant()} von außen zu {forward.TargetAddress}:{forward.InternalPort} weiterleiten („{forward.Name}\")",
                (client, ct) => client.AddAsync("/ip/firewall/nat", new Dictionary<string, string>
                {
                    ["chain"] = "dstnat",
                    ["protocol"] = forward.Protocol,
                    ["dst-port"] = forward.ExternalPort.ToString(),
                    ["in-interface"] = settings.WanInterfaceName,
                    ["action"] = "dst-nat",
                    ["to-addresses"] = forward.TargetAddress,
                    ["to-ports"] = forward.InternalPort.ToString(),
                }, ct)));

            actions.Add(new ConfigurationAction(
                $"Weitergeleiteten Datenverkehr für „{forward.Name}\" durch die Firewall lassen",
                (client, ct) => client.AddAsync("/ip/firewall/filter", new Dictionary<string, string>
                {
                    ["chain"] = "forward",
                    ["protocol"] = forward.Protocol,
                    ["dst-address"] = forward.TargetAddress,
                    ["dst-port"] = forward.InternalPort.ToString(),
                    ["action"] = "accept",
                }, ct)));
        }

        actions.Add(ForwardRule(
            "Bereits bestehende Verbindungen durch den Router weiterhin erlauben",
            new Dictionary<string, string> { ["connection-state"] = "established,related,untracked", ["action"] = "accept" }));
        actions.Add(ForwardRule(
            "Ungültige weitergeleitete Verbindungen verwerfen",
            new Dictionary<string, string> { ["connection-state"] = "invalid", ["action"] = "drop" }));
        actions.Add(ForwardRule(
            $"Unangeforderte neue Verbindungen aus dem Internet („{settings.WanInterfaceName}\") blockieren, sofern nicht ausdrücklich freigegeben",
            new Dictionary<string, string> { ["in-interface"] = settings.WanInterfaceName, ["connection-state"] = "new", ["action"] = "drop" }));

        actions.Add(InputRule(
            $"Zugriff auf die Router-Einstellungen aus dem Internet („{settings.WanInterfaceName}\") blockieren",
            new Dictionary<string, string> { ["in-interface"] = settings.WanInterfaceName, ["action"] = "drop" }));

        return actions;
    }

    private static ConfigurationAction InputRule(string description, Dictionary<string, string> parameters) =>
        ChainRule("input", description, parameters);

    private static ConfigurationAction ForwardRule(string description, Dictionary<string, string> parameters) =>
        ChainRule("forward", description, parameters);

    private static ConfigurationAction ChainRule(string chain, string description, Dictionary<string, string> parameters)
    {
        parameters["chain"] = chain;
        return new ConfigurationAction(description, (client, ct) => client.AddAsync("/ip/firewall/filter", parameters, ct));
    }
}
