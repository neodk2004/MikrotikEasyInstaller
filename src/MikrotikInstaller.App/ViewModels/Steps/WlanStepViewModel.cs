using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MikrotikInstaller.Core.Configuration;
using MikrotikInstaller.Core.Devices;

namespace MikrotikInstaller.App.ViewModels.Steps;

/// <summary>
/// Sechster Schritt: WLAN-Name (SSID) und Passwort. Wird komplett übersprungen, wenn das Gerät gar
/// keine WLAN-Schnittstelle hat. Hat das Gerät zwar WLAN-Hardware, aber deaktiviert (z. B. weil es
/// hier rein als Switch dient), kann der Nutzer den Schritt trotzdem bewusst überspringen.
/// </summary>
public partial class WlanStepViewModel : WizardStepViewModelBase
{
    private readonly WizardSession _session;

    public WlanStepViewModel(WizardSession session)
        : base(
            title: "WLAN",
            description: "Lege den Namen (SSID) und das Passwort für dein WLAN fest. Das Passwort muss mindestens 8 Zeichen lang sein.")
    {
        _session = session;
    }

    public ObservableCollection<string> AvailableInterfaces { get; } = [];

    [ObservableProperty]
    private string? _selectedInterface;

    [ObservableProperty]
    private string _ssid = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _skipWireless;

    public override bool ShouldSkip => _session.Device?.HasWireless != true;

    public override bool CanGoNext =>
        SkipWireless
        || (!string.IsNullOrWhiteSpace(SelectedInterface) && !string.IsNullOrWhiteSpace(Ssid) && Password.Length >= 8);

    public override Task OnActivatedAsync()
    {
        AvailableInterfaces.Clear();
        foreach (var iface in _session.Device?.Interfaces.Where(i => i.Kind == InterfaceKind.Wireless) ?? [])
        {
            AvailableInterfaces.Add(iface.Name);
        }

        SelectedInterface ??= AvailableInterfaces.FirstOrDefault();
        SaveToSession();
        return Task.CompletedTask;
    }

    partial void OnSelectedInterfaceChanged(string? value) => SaveToSession();

    partial void OnSsidChanged(string value) => SaveToSession();

    partial void OnPasswordChanged(string value) => SaveToSession();

    partial void OnSkipWirelessChanged(bool value) => SaveToSession();

    private void SaveToSession()
    {
        OnPropertyChanged(nameof(CanGoNext));

        if (SkipWireless || !CanGoNext)
        {
            _session.WirelessActions = null;
            _session.WirelessSettings = null;
            return;
        }

        var settings = new WirelessSettings(SelectedInterface!, Ssid, Password);
        _session.WirelessSettings = settings;
        _session.WirelessActions = WirelessConfigurator.BuildActions(settings);
    }
}
