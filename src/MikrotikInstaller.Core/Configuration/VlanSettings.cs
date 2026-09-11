namespace MikrotikInstaller.Core.Configuration;

/// <summary>Ein zusätzliches, vom Heimnetz getrenntes Netzwerk (eigenes Subnetz + eigener DHCP-Server).</summary>
public sealed record VlanDefinition(
    int VlanId,
    string Name,
    string RouterAddressCidr,
    string DhcpPoolStart,
    string DhcpPoolEnd);
