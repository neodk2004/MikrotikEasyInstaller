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
            new VlanStepViewModel(_session),
            new WlanStepViewModel(_session),
            new FirewallStepViewModel(_session),
            new SummaryStepViewModel(_session),
            new UpdateStepViewModel(_session),
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
    [NotifyPropertyChangedFor(nameof(StepNavItems))]
    [NotifyCanExecuteChangedFor(nameof(GoBackCommand))]
    [NotifyCanExecuteChangedFor(nameof(GoNextCommand))]
    private int _currentStepIndex;

    public WizardStepViewModelBase CurrentStep => Steps[CurrentStepIndex];

    public string ProgressText => $"Schritt {CurrentStepIndex + 1} von {Steps.Count}";

    public IReadOnlyList<StepNavItem> StepNavItems => Steps
        .Select((step, index) => new StepNavItem(index + 1, step.Title, index == CurrentStepIndex, index < CurrentStepIndex))
        .ToList();

    public bool IsFirstStep => CurrentStepIndex == 0;

    public bool IsLastStep => CurrentStepIndex == Steps.Count - 1;

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    private async Task GoBackAsync()
    {
        do
        {
            CurrentStepIndex--;
        }
        while (CurrentStepIndex > 0 && CurrentStep.ShouldSkip);

        await CurrentStep.OnActivatedAsync();
    }

    private bool CanGoBack() => !IsFirstStep;

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private async Task GoNextAsync()
    {
        do
        {
            CurrentStepIndex++;
        }
        while (CurrentStepIndex < Steps.Count - 1 && CurrentStep.ShouldSkip);

        await CurrentStep.OnActivatedAsync();
    }

    private bool CanGoNext() => !IsLastStep && CurrentStep.CanGoNext;

    /// <summary>Wird beim Schließen des Hauptfensters aufgerufen, um die Geräteverbindung sauber zu beenden.</summary>
    public Task DisposeSessionAsync() => _session.DisposeClientAsync();
}
