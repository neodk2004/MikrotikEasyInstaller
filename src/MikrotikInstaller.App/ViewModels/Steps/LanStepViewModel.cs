using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MikrotikInstaller.Core.Configuration;
using MikrotikInstaller.Core.Devices;

namespace MikrotikInstaller.App.ViewModels.Steps;

/// <summary>
/// Vierter Schritt: Zusammenstellung des Heimnetzwerks (Bridge, Mitglieds-Schnittstellen, IP-Bereich,
/// DHCP-Server, DNS). Wie beim WAN-Schritt wird hier nur der Änderungsplan in der Session gesammelt.
/// </summary>
public partial class LanStepViewModel : WizardStepViewModelBase
{
    private readonly WizardSession _session;

    public LanStepViewModel(WizardSession session)
        : base(
            title: "Heimnetzwerk (DHCP)",
            description: "Wähle, welche Anschlüsse zu deinem Heimnetzwerk gehören sollen, und lege den IP-Adressbereich fest. Die Vorgaben passen für die meisten Haushalte.")
    {
        _session = session;
    }

    public ObservableCollection<SelectableInterface> AvailableInterfaces { get; } = [];

    [ObservableProperty]
    private string _bridgeName = "bridge-lan";

    [ObservableProperty]
    private string _routerAddress = "192.168.88.1";

    [ObservableProperty]
    private int _prefixLength = 24;

    [ObservableProperty]
    private string _dhcpPoolStart = "192.168.88.10";

    [ObservableProperty]
    private string _dhcpPoolEnd = "192.168.88.254";

    [ObservableProperty]
    private string _primaryDns = "1.1.1.1";

    [ObservableProperty]
    private string _secondaryDns = "8.8.8.8";

    public override bool CanGoNext =>
        AvailableInterfaces.Any(i => i.IsSelected)
        && !string.IsNullOrWhiteSpace(BridgeName)
        && !string.IsNullOrWhiteSpace(RouterAddress)
        && !string.IsNullOrWhiteSpace(DhcpPoolStart)
        && !string.IsNullOrWhiteSpace(DhcpPoolEnd)
        && !string.IsNullOrWhiteSpace(PrimaryDns);

    public override Task OnActivatedAsync()
    {
        AvailableInterfaces.Clear();
        var candidates = (_session.Device?.Interfaces ?? []).Where(
            i => i.Kind == InterfaceKind.Ethernet && i.Name != _session.WanInterfaceName);

        foreach (var iface in candidates)
        {
            var item = new SelectableInterface(iface.Name, isSelected: true);
            item.PropertyChanged += (_, _) => SaveToSession();
            AvailableInterfaces.Add(item);
        }

        SaveToSession();
        return Task.CompletedTask;
    }

    partial void OnBridgeNameChanged(string value) => SaveToSession();

    partial void OnRouterAddressChanged(string value) => SaveToSession();

    partial void OnPrefixLengthChanged(int value) => SaveToSession();

    partial void OnDhcpPoolStartChanged(string value) => SaveToSession();

    partial void OnDhcpPoolEndChanged(string value) => SaveToSession();

    partial void OnPrimaryDnsChanged(string value) => SaveToSession();

    partial void OnSecondaryDnsChanged(string value) => SaveToSession();

    private void SaveToSession()
    {
        OnPropertyChanged(nameof(CanGoNext));

        if (!CanGoNext)
        {
            _session.LanActions = null;
            _session.LanBridgeName = null;
            return;
        }

        _session.LanBridgeName = BridgeName;

        var settings = new LanSettings(
            BridgeName,
            AvailableInterfaces.Where(i => i.IsSelected).Select(i => i.Name).ToList(),
            $"{RouterAddress}/{PrefixLength}",
            DhcpPoolStart,
            DhcpPoolEnd,
            PrimaryDns,
            string.IsNullOrWhiteSpace(SecondaryDns) ? null : SecondaryDns);

        _session.LanActions = LanConfigurator.BuildActions(settings);
    }
}
