using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using MikrotikInstaller.Core.Configuration;

namespace MikrotikInstaller.App.ViewModels.Steps;

/// <summary>
/// Siebter Schritt: sichere Basis-Firewallregeln (immer angewendet) plus optionale Portfreigaben
/// für einzelne Geräte im Heimnetz (z. B. Spiele- oder NAS-Server).
/// </summary>
public partial class FirewallStepViewModel : WizardStepViewModelBase
{
    private readonly WizardSession _session;

    public FirewallStepViewModel(WizardSession session)
        : base(
            title: "Firewall",
            description: "Wir richten sichere Basis-Firewallregeln ein: Der Router ist aus dem Internet nicht erreichbar, dein Heimnetz bleibt geschützt. Optional kannst du einzelne Ports für Geräte in deinem Netzwerk freigeben.")
    {
        _session = session;
    }

    public ObservableCollection<PortForwardEntry> PortForwards { get; } = [];

    public IReadOnlyList<string> Protocols { get; } = ["tcp", "udp"];

    public override bool CanGoNext => PortForwards.All(IsValid);

    public override Task OnActivatedAsync()
    {
        SaveToSession();
        return Task.CompletedTask;
    }

    [RelayCommand]
    private void AddPortForward()
    {
        var entry = new PortForwardEntry();
        entry.PropertyChanged += (_, _) => SaveToSession();
        PortForwards.Add(entry);
        SaveToSession();
    }

    [RelayCommand]
    private void RemovePortForward(PortForwardEntry entry)
    {
        PortForwards.Remove(entry);
        SaveToSession();
    }

    private static bool IsValid(PortForwardEntry entry) =>
        !string.IsNullOrWhiteSpace(entry.Name)
        && !string.IsNullOrWhiteSpace(entry.TargetAddress)
        && int.TryParse(entry.ExternalPort, out var externalPort) && externalPort is > 0 and <= 65535
        && int.TryParse(entry.InternalPort, out var internalPort) && internalPort is > 0 and <= 65535;

    private void SaveToSession()
    {
        OnPropertyChanged(nameof(CanGoNext));

        if (!CanGoNext || _session.WanInterfaceName is null || _session.LanBridgeName is null)
        {
            _session.FirewallActions = null;
            return;
        }

        var forwards = PortForwards
            .Select(e => new PortForward(e.Name, e.Protocol, int.Parse(e.ExternalPort), e.TargetAddress, int.Parse(e.InternalPort)))
            .ToList();

        var settings = new FirewallSettings(_session.WanInterfaceName, _session.LanBridgeName, forwards);
        _session.FirewallActions = FirewallConfigurator.BuildActions(settings);
    }
}
