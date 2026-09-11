using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MikrotikInstaller.Core.Connectivity;
using MikrotikInstaller.Core.Updates;

namespace MikrotikInstaller.App.ViewModels.Steps;

/// <summary>
/// Letzter, optionaler Schritt: zeigt, ob eine neuere RouterOS-Version verfügbar ist, und kann sie
/// auf Wunsch installieren. Komplett optional — der Assistent gilt bereits mit dem vorherigen Schritt
/// als abgeschlossen.
/// </summary>
public partial class UpdateStepViewModel : WizardStepViewModelBase
{
    private readonly WizardSession _session;

    public UpdateStepViewModel(WizardSession session)
        : base(
            title: "RouterOS-Update",
            description: "Falls eine neuere RouterOS-Version verfügbar ist, kannst du sie hier optional installieren. Das ist kein Muss — dein Gerät ist bereits fertig eingerichtet.")
    {
        _session = session;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowCheckFailed))]
    [NotifyPropertyChangedFor(nameof(ShowUpToDate))]
    [NotifyPropertyChangedFor(nameof(ShowUpdateAvailable))]
    private bool _isChecking;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowCheckFailed))]
    [NotifyPropertyChangedFor(nameof(ShowUpToDate))]
    [NotifyPropertyChangedFor(nameof(ShowUpdateAvailable))]
    private bool _isCheckSuccessful;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowUpToDate))]
    [NotifyPropertyChangedFor(nameof(ShowUpdateAvailable))]
    private bool _isUpdateAvailable;

    [ObservableProperty]
    private string? _installedVersion;

    [ObservableProperty]
    private string? _latestVersion;

    [ObservableProperty]
    private bool _confirmReboot;

    [ObservableProperty]
    private bool _isUpdating;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowUpToDate))]
    [NotifyPropertyChangedFor(nameof(ShowUpdateAvailable))]
    private bool _isUpdateStarted;

    [ObservableProperty]
    private string? _errorMessage;

    public bool ShowCheckFailed => !IsChecking && !IsCheckSuccessful;

    public bool ShowUpToDate => !IsChecking && IsCheckSuccessful && !IsUpdateAvailable && !IsUpdateStarted;

    public bool ShowUpdateAvailable => !IsChecking && IsCheckSuccessful && IsUpdateAvailable && !IsUpdateStarted;

    public override Task OnActivatedAsync()
    {
        ApplyInfo(_session.UpdateInfo);
        return Task.CompletedTask;
    }

    private void ApplyInfo(RouterOsUpdateInfo? info)
    {
        IsCheckSuccessful = info?.IsCheckSuccessful ?? false;
        IsUpdateAvailable = info?.IsUpdateAvailable ?? false;
        InstalledVersion = string.IsNullOrWhiteSpace(info?.InstalledVersion) ? null : info.InstalledVersion;
        LatestVersion = info?.LatestVersion;
        StartUpdateCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private async Task RecheckAsync()
    {
        if (_session.Client is null)
        {
            return;
        }

        IsChecking = true;
        ErrorMessage = null;

        try
        {
            var info = await RouterOsUpdateService.CheckForUpdatesAsync(_session.Client);
            _session.UpdateInfo = info;
            ApplyInfo(info);
        }
        finally
        {
            IsChecking = false;
        }
    }

    private bool CanStartUpdate => IsUpdateAvailable && ConfirmReboot && !IsUpdating && !IsUpdateStarted;

    partial void OnConfirmRebootChanged(bool value) => StartUpdateCommand.NotifyCanExecuteChanged();

    [RelayCommand(CanExecute = nameof(CanStartUpdate))]
    private async Task StartUpdateAsync()
    {
        if (_session.Client is null)
        {
            return;
        }

        IsUpdating = true;
        ErrorMessage = null;

        try
        {
            await RouterOsUpdateService.StartUpdateAsync(_session.Client);
            IsUpdateStarted = true;
        }
        catch (RouterOsException ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsUpdating = false;
            StartUpdateCommand.NotifyCanExecuteChanged();
        }
    }
}
