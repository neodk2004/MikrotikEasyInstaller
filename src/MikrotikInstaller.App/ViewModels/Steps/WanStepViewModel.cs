using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MikrotikInstaller.Core.Configuration;
using MikrotikInstaller.Core.Devices;

namespace MikrotikInstaller.App.ViewModels.Steps;

/// <summary>
/// Dritter Schritt: Wahl der Internet-Schnittstelle (WAN) und ob die Adresse automatisch (DHCP)
/// oder fest vergeben werden soll. Es wird hier noch nichts am Gerät verändert — die daraus
/// resultierenden Änderungen werden nur in der Session gesammelt und erst im
/// Zusammenfassungs-Schritt angewendet.
/// </summary>
public partial class WanStepViewModel : WizardStepViewModelBase
{
    private readonly WizardSession _session;

    public WanStepViewModel(WizardSession session)
        : base(
            title: "Internet-Zugang",
            description: "Wähle die Schnittstelle, an der dein Internet-Kabel (vom Modem/Router deines Anbieters) angeschlossen ist.")
    {
        _session = session;
    }

    public ObservableCollection<string> AvailableInterfaces { get; } = [];

    [ObservableProperty]
    private string? _selectedInterface;

    [ObservableProperty]
    private bool _useStaticAddress;

    [ObservableProperty]
    private string _staticAddress = string.Empty;

    [ObservableProperty]
    private string _staticGateway = string.Empty;

    public override bool CanGoNext =>
        !string.IsNullOrWhiteSpace(SelectedInterface)
        && (!UseStaticAddress || (!string.IsNullOrWhiteSpace(StaticAddress) && !string.IsNullOrWhiteSpace(StaticGateway)));

    public override Task OnActivatedAsync()
    {
        AvailableInterfaces.Clear();
        foreach (var iface in _session.Device?.Interfaces.Where(i => i.Kind == InterfaceKind.Ethernet) ?? [])
        {
            AvailableInterfaces.Add(iface.Name);
        }

        SelectedInterface ??= AvailableInterfaces.FirstOrDefault();
        SaveToSession();
        return Task.CompletedTask;
    }

    partial void OnSelectedInterfaceChanged(string? value) => SaveToSession();

    partial void OnUseStaticAddressChanged(bool value) => SaveToSession();

    partial void OnStaticAddressChanged(string value) => SaveToSession();

    partial void OnStaticGatewayChanged(string value) => SaveToSession();

    private void SaveToSession()
    {
        OnPropertyChanged(nameof(CanGoNext));

        _session.WanInterfaceName = SelectedInterface;

        if (!CanGoNext)
        {
            _session.WanActions = null;
            _session.WanSettings = null;
            return;
        }

        var settings = UseStaticAddress
            ? new WanSettings(SelectedInterface!, WanAddressMode.Static, StaticAddress, StaticGateway)
            : new WanSettings(SelectedInterface!, WanAddressMode.Dhcp);

        _session.WanSettings = settings;
        _session.WanActions = WanConfigurator.BuildActions(settings);
    }
}
