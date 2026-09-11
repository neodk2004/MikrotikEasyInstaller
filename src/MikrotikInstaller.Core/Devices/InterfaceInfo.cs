namespace MikrotikInstaller.Core.Devices;

public sealed record InterfaceInfo(
    string Name,
    InterfaceKind Kind,
    bool IsRunning,
    bool IsDisabled,
    string? Comment);
