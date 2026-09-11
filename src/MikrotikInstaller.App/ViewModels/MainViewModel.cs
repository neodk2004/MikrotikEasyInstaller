using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MikrotikInstaller.App.ViewModels.Steps;

namespace MikrotikInstaller.App.ViewModels;

/// <summary>
/// Steuert die Schritt-für-Schritt-Navigation des Einrichtungs-Assistenten.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    public MainViewModel()
    {
        Steps =
        [
            new ConnectionStepViewModel(),
            new PlaceholderStepViewModel("Geräte-Erkennung", "Modell, RouterOS-Version und Schnittstellen werden ausgelesen."),
            new PlaceholderStepViewModel("Internet-Zugang", "WAN-Schnittstelle und Internetverbindung einrichten."),
            new PlaceholderStepViewModel("Heimnetzwerk (DHCP)", "IP-Adressbereich und DHCP-Server für dein Netzwerk festlegen."),
            new PlaceholderStepViewModel("VLANs", "Netzwerke logisch voneinander trennen."),
            new PlaceholderStepViewModel("WLAN", "WLAN-Name und Passwort festlegen."),
            new PlaceholderStepViewModel("Firewall", "Sichere Basis-Firewallregeln auswählen."),
            new PlaceholderStepViewModel("Zusammenfassung", "Alle geplanten Änderungen im Überblick, bevor etwas angewendet wird."),
        ];

        CurrentStepIndex = 0;
    }

    public ObservableCollection<WizardStepViewModelBase> Steps { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentStep))]
    [NotifyPropertyChangedFor(nameof(ProgressText))]
    [NotifyPropertyChangedFor(nameof(IsFirstStep))]
    [NotifyPropertyChangedFor(nameof(IsLastStep))]
    [NotifyCanExecuteChangedFor(nameof(GoBackCommand))]
    [NotifyCanExecuteChangedFor(nameof(GoNextCommand))]
    private int _currentStepIndex;

    public WizardStepViewModelBase CurrentStep => Steps[CurrentStepIndex];

    public string ProgressText => $"Schritt {CurrentStepIndex + 1} von {Steps.Count}";

    public bool IsFirstStep => CurrentStepIndex == 0;

    public bool IsLastStep => CurrentStepIndex == Steps.Count - 1;

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    private void GoBack() => CurrentStepIndex--;

    private bool CanGoBack() => !IsFirstStep;

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private void GoNext() => CurrentStepIndex++;

    private bool CanGoNext() => !IsLastStep && CurrentStep.CanGoNext;
}
