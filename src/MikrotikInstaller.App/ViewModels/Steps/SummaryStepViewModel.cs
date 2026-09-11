using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MikrotikInstaller.Core.Backup;
using MikrotikInstaller.Core.Configuration;
using MikrotikInstaller.Core.Connectivity;

namespace MikrotikInstaller.App.ViewModels.Steps;

/// <summary>
/// Letzter Schritt: zeigt alle in den vorherigen Schritten geplanten Änderungen im Klartext an.
/// Erst wenn der Nutzer hier ausdrücklich "Jetzt einrichten" klickt, wird ein Backup erstellt und
/// werden die Änderungen tatsächlich auf dem Gerät angewendet.
/// </summary>
public partial class SummaryStepViewModel : WizardStepViewModelBase
{
    private readonly WizardSession _session;

    public SummaryStepViewModel(WizardSession session)
        : base(
            title: "Zusammenfassung",
            description: "Das sind alle Änderungen, die wir jetzt an deinem Gerät vornehmen. Vor dem Anwenden erstellen wir automatisch ein Backup auf dem Gerät, falls du etwas rückgängig machen möchtest.")
    {
        _session = session;
    }

    public ObservableCollection<string> PlannedChanges { get; } = [];

    [ObservableProperty]
    private bool _isApplying;

    [ObservableProperty]
    private string? _progressText;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isFinished;

    [ObservableProperty]
    private string? _backupName;

    public override Task OnActivatedAsync()
    {
        PlannedChanges.Clear();
        foreach (var action in GetAllActions())
        {
            PlannedChanges.Add(action.Description);
        }

        IsFinished = false;
        ErrorMessage = null;
        ApplyCommand.NotifyCanExecuteChanged();
        return Task.CompletedTask;
    }

    private bool CanApply => !IsApplying && !IsFinished && _session.Client is not null && PlannedChanges.Count > 0;

    [RelayCommand(CanExecute = nameof(CanApply))]
    private async Task ApplyAsync()
    {
        if (_session.Client is null)
        {
            return;
        }

        IsApplying = true;
        ErrorMessage = null;
        ApplyCommand.NotifyCanExecuteChanged();

        try
        {
            ProgressText = "Sicherung wird auf dem Gerät erstellt …";
            BackupName = await RouterBackupService.CreateBackupAsync(_session.Client);

            var actions = GetAllActions();
            for (var i = 0; i < actions.Count; i++)
            {
                ProgressText = $"Schritt {i + 1} von {actions.Count}: {actions[i].Description}";
                await actions[i].ApplyAsync(_session.Client, CancellationToken.None);
            }

            ProgressText = null;
            IsFinished = true;
        }
        catch (RouterOsException ex)
        {
            ErrorMessage = BackupName is null
                ? ex.Message
                : $"{ex.Message} Falls dein Gerät jetzt nicht wie erwartet funktioniert, kannst du auf dem Gerät selbst (z. B. über WinBox → Files, oder das Gerätedisplay) das automatisch erstellte Backup „{BackupName}\" wiederherstellen.";
        }
        finally
        {
            IsApplying = false;
            ApplyCommand.NotifyCanExecuteChanged();
        }
    }

    private IReadOnlyList<ConfigurationAction> GetAllActions() =>
        (_session.WanActions ?? [])
        .Concat(_session.LanActions ?? [])
        .Concat(_session.VlanActions ?? [])
        .Concat(_session.WirelessActions ?? [])
        .Concat(_session.FirewallActions ?? [])
        .ToList();
}
