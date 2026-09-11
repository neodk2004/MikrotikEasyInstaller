namespace MikrotikInstaller.Core.Configuration;

public sealed record LanSettings(
    string BridgeName,
    IReadOnlyList<string> MemberInterfaces,
    string RouterAddressCidr,
    string DhcpPoolStart,
    string DhcpPoolEnd,
    string PrimaryDnsServer,
    string? SecondaryDnsServer);
