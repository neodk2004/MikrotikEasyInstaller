using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using MikrotikInstaller.Core.Configuration;

namespace MikrotikInstaller.App.ViewModels.Steps;

/// <summary>
/// Fünfter Schritt: optionale, zusätzliche Netzwerke (VLANs), z. B. für Gäste oder smarte Geräte.
/// Ohne Einträge wird dieser Schritt einfach übersprungen, ohne dass sich am Gerät etwas ändert.
/// </summary>
public partial class VlanStepViewModel : WizardStepViewModelBase
{
    private readonly WizardSession _session;
    private int _nextSubnetOctet = 20;

    public VlanStepViewModel(WizardSession session)
        : base(
            title: "VLANs",
            description: "Optional: Lege zusätzliche, vom Heimnetz getrennte Netzwerke an, z. B. für Gäste oder smarte Geräte. Füge keinen Eintrag hinzu, wenn du das nicht brauchst.")
    {
        _session = session;
    }

    public ObservableCollection<VlanEntry> Vlans { get; } = [];

    public override bool CanGoNext => Vlans.All(IsValid);

    public override Task OnActivatedAsync()
    {
        SaveToSession();
        return Task.CompletedTask;
    }

    [RelayCommand]
    private void AddVlan()
    {
        var octet = _nextSubnetOctet;
        _nextSubnetOctet += 10;

        var entry = new VlanEntry
        {
            VlanId = (10 + (Vlans.Count * 10)).ToString(),
            Name = $"Netzwerk {Vlans.Count + 1}",
            RouterAddress = $"192.168.{octet}.1",
            PoolStart = $"192.168.{octet}.10",
            PoolEnd = $"192.168.{octet}.254",
        };
        entry.PropertyChanged += (_, _) => SaveToSession();
        Vlans.Add(entry);
        SaveToSession();
    }

    [RelayCommand]
    private void RemoveVlan(VlanEntry entry)
    {
        Vlans.Remove(entry);
        SaveToSession();
    }

    private static bool IsValid(VlanEntry entry) =>
        int.TryParse(entry.VlanId, out var id) && id is >= 2 and <= 4094
        && !string.IsNullOrWhiteSpace(entry.Name)
        && !string.IsNullOrWhiteSpace(entry.RouterAddress)
        && !string.IsNullOrWhiteSpace(entry.PoolStart)
        && !string.IsNullOrWhiteSpace(entry.PoolEnd);

    private void SaveToSession()
    {
        OnPropertyChanged(nameof(CanGoNext));

        if (!CanGoNext || _session.LanBridgeName is null)
        {
            _session.VlanActions = null;
            _session.VlanDefinitions = null;
            return;
        }

        var definitions = Vlans
            .Select(v => new VlanDefinition(int.Parse(v.VlanId), v.Name, $"{v.RouterAddress}/24", v.PoolStart, v.PoolEnd))
            .ToList();

        _session.VlanDefinitions = definitions;
        _session.VlanActions = VlanConfigurator.BuildActions(_session.LanBridgeName, definitions);
    }
}
