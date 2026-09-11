using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MikrotikInstaller.Core.Connectivity;
using MikrotikInstaller.Core.Devices;
using MikrotikInstaller.Core.Updates;

namespace MikrotikInstaller.App.ViewModels.Steps;

/// <summary>
/// Zweiter Schritt: liest beim Betreten automatisch Modell, RouterOS-Version und Schnittstellen
/// des zuvor verbundenen Geräts aus.
/// </summary>
public partial class DeviceOverviewStepViewModel : WizardStepViewModelBase
{
    private readonly WizardSession _session;

    public DeviceOverviewStepViewModel(WizardSession session)
        : base(
            title: "Geräte-Erkennung",
            description: "Wir lesen Modell, RouterOS-Version und Netzwerkschnittstellen deines Geräts aus.")
    {
        _session = session;
    }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoNext))]
    [NotifyPropertyChangedFor(nameof(Interfaces))]
    private RouterDevice? _device;

    public override bool CanGoNext => Device is not null;

    public IReadOnlyList<InterfaceDisplayItem> Interfaces =>
        Device?.Interfaces.Select(InterfaceDisplayItem.From).ToList() ?? [];

    public override Task OnActivatedAsync() => LoadDeviceAsync();

    [RelayCommand]
    private Task Retry() => LoadDeviceAsync();

    private async Task LoadDeviceAsync()
    {
        if (_session.Client is null)
        {
            ErrorMessage = "Es besteht keine Verbindung zum Gerät. Bitte gehe einen Schritt zurück.";
            Device = null;
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var device = await DeviceDiscoveryService.DiscoverAsync(_session.Client);
            Device = device;
            _session.Device = device;

            // Läuft im Hintergrund weiter, ohne die Navigation zu blockieren — ein frisches Gerät
            // hat an dieser Stelle im Assistenten meist noch keinen Internetzugang, die Prüfung
            // schlägt dann einfach fehl (kein Fehlerzustand) und wird nach dem Anwenden wiederholt.
            _ = CheckForUpdatesInBackgroundAsync(_session.Client);
        }
        catch (RouterOsException ex)
        {
            Device = null;
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task CheckForUpdatesInBackgroundAsync(IRouterOsClient client)
    {
        _session.UpdateInfo = await RouterOsUpdateService.CheckForUpdatesAsync(client);
    }
}
