namespace MikrotikInstaller.Core.Configuration;

/// <summary>Eine Portfreigabe: leitet einen Port von außen zu einem bestimmten Gerät im Heimnetz weiter.</summary>
public sealed record PortForward(string Name, string Protocol, int ExternalPort, string TargetAddress, int InternalPort);

public sealed record FirewallSettings(
    string WanInterfaceName,
    string LanBridgeName,
    IReadOnlyList<PortForward> PortForwards);
