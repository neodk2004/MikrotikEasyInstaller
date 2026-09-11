namespace MikrotikInstaller.App.ViewModels.Steps;

/// <summary>
/// Erster Schritt: Verbindungsdaten zum MikroTik-Gerät erfassen.
/// Die eigentliche Verbindungslogik (RouterOsClientFactory) kommt in Phase 2.
/// </summary>
public partial class ConnectionStepViewModel : WizardStepViewModelBase
{
    public ConnectionStepViewModel()
        : base(
            title: "Verbindung",
            description: "Gib die Adresse deines MikroTik-Geräts sowie Benutzername und Passwort ein.")
    {
    }
}
