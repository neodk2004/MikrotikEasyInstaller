using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MikrotikInstaller.Core.Connectivity;

namespace MikrotikInstaller.App.ViewModels.Steps;

/// <summary>
/// Erster Schritt: Verbindungsdaten zum MikroTik-Gerät erfassen und testen.
/// Erst nach einer erfolgreich getesteten Verbindung darf im Assistenten weitergegangen werden.
/// </summary>
public partial class ConnectionStepViewModel : WizardStepViewModelBase
{
    public ConnectionStepViewModel()
        : base(
            title: "Verbindung",
            description: "Gib die Adresse deines MikroTik-Geräts sowie Benutzername und Passwort ein.")
    {
    }

    [ObservableProperty]
    private string _host = string.Empty;

    [ObservableProperty]
    private string _username = "admin";

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _useEncryption;

    [ObservableProperty]
    private bool _isTesting;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _statusIsError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoNext))]
    private bool _isConnected;

    public override bool CanGoNext => IsConnected;

    private bool CanTestConnection => !IsTesting && !string.IsNullOrWhiteSpace(Host) && !string.IsNullOrWhiteSpace(Username);

    [RelayCommand(CanExecute = nameof(CanTestConnection))]
    private async Task TestConnectionAsync()
    {
        IsTesting = true;
        StatusMessage = "Verbindung wird getestet …";
        StatusIsError = false;
        IsConnected = false;

        try
        {
            var credentials = new RouterOsCredentials(Host.Trim(), Username.Trim(), Password)
            {
                UseEncryption = UseEncryption,
            };

            await using var client = await RouterOsClientFactory.ConnectAsync(credentials);

            IsConnected = true;
            StatusIsError = false;
            StatusMessage = "Verbindung erfolgreich hergestellt.";
        }
        catch (RouterOsException ex)
        {
            IsConnected = false;
            StatusIsError = true;
            StatusMessage = ex.Message;
        }
        finally
        {
            IsTesting = false;
        }
    }

    partial void OnHostChanged(string value) => TestConnectionCommand.NotifyCanExecuteChanged();

    partial void OnUsernameChanged(string value) => TestConnectionCommand.NotifyCanExecuteChanged();

    partial void OnIsTestingChanged(bool value) => TestConnectionCommand.NotifyCanExecuteChanged();
}
