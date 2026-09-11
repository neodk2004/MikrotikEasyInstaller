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
    private readonly WizardSession _session = new();

    public MainViewModel()
    {
        Steps =
        [
            new ConnectionStepViewModel(_session),
            new DeviceOverviewStepViewModel(_session),
            new WanStepViewModel(_session),
            new LanStepViewModel(_session),
            new PlaceholderStepViewModel("VLANs", "Netzwerke logisch voneinander trennen."),
            new PlaceholderStepViewModel("WLAN", "WLAN-Name und Passwort festlegen."),
            new PlaceholderStepViewModel("Firewall", "Sichere Basis-Firewallregeln auswählen."),
            new PlaceholderStepViewModel("Zusammenfassung", "Alle geplanten Änderungen im Überblick, bevor etwas angewendet wird."),
        ];

        foreach (var step in Steps)
        {
            step.PropertyChanged += (_, _) => GoNextCommand.NotifyCanExecuteChanged();
        }

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
    private async Task GoBackAsync()
    {
        CurrentStepIndex--;
        await CurrentStep.OnActivatedAsync();
    }

    private bool CanGoBack() => !IsFirstStep;

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private async Task GoNextAsync()
    {
        CurrentStepIndex++;
        await CurrentStep.OnActivatedAsync();
    }

    private bool CanGoNext() => !IsLastStep && CurrentStep.CanGoNext;

    /// <summary>Wird beim Schließen des Hauptfensters aufgerufen, um die Geräteverbindung sauber zu beenden.</summary>
    public Task DisposeSessionAsync() => _session.DisposeClientAsync();
}
