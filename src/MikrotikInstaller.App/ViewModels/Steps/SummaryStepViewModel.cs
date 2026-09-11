using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MikrotikInstaller.Core.Backup;
using MikrotikInstaller.Core.Configuration;
using MikrotikInstaller.Core.Connectivity;
using MikrotikInstaller.Core.Export;
using Wpf.Ui.Controls;

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

    public ObservableCollection<SummarySection> Sections { get; } = [];

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

    [ObservableProperty]
    private string? _pdfExportMessage;

    [ObservableProperty]
    private bool _pdfExportIsError;

    public override Task OnActivatedAsync()
    {
        Sections.Clear();

        if (_session.WanSettings is { } wan)
        {
            Sections.Add(new SummarySection("Internet-Zugang", SymbolRegular.Globe24, WanConfigurator.BuildFriendlySummary(wan)));
        }

        if (_session.LanSettings is { } lan)
        {
            Sections.Add(new SummarySection("Heimnetzwerk", SymbolRegular.Home24, LanConfigurator.BuildFriendlySummary(lan)));
        }

        if (_session.VlanDefinitions is { Count: > 0 } vlans)
        {
            Sections.Add(new SummarySection("Zusätzliche Netzwerke", SymbolRegular.HomeSplit24, VlanConfigurator.BuildFriendlySummary(vlans)));
        }

        if (_session.WirelessSettings is { } wireless)
        {
            Sections.Add(new SummarySection("WLAN", SymbolRegular.Wifi124, WirelessConfigurator.BuildFriendlySummary(wireless)));
        }

        if (_session.FirewallSettings is { } firewall)
        {
            Sections.Add(new SummarySection("Firewall", SymbolRegular.ShieldCheckmark24, FirewallConfigurator.BuildFriendlySummary(firewall)));
        }

        IsFinished = false;
        ErrorMessage = null;
        PdfExportMessage = null;
        ApplyCommand.NotifyCanExecuteChanged();
        ExportPdfCommand.NotifyCanExecuteChanged();
        return Task.CompletedTask;
    }

    private bool CanApply => !IsApplying && !IsFinished && _session.Client is not null && Sections.Count > 0;

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

    private bool CanExportPdf => Sections.Count > 0;

    [RelayCommand(CanExecute = nameof(CanExportPdf))]
    private void ExportPdf()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "Einrichtungsprotokoll speichern",
            Filter = "PDF-Datei (*.pdf)|*.pdf",
            FileName = $"MikroTik-Einrichtung-{DateTime.Now:yyyy-MM-dd}.pdf",
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            var data = new SummaryDocumentData(
                _session.Device?.IdentityName ?? "MikroTik",
                _session.Device?.BoardModel ?? "unbekannt",
                _session.WanSettings,
                _session.LanSettings,
                _session.VlanDefinitions ?? [],
                _session.WirelessSettings,
                _session.FirewallSettings,
                BackupName);

            SummaryPdfExporter.Export(data, dialog.FileName);

            PdfExportIsError = false;
            PdfExportMessage = $"Gespeichert unter „{dialog.FileName}\".";
        }
        catch (Exception ex)
        {
            PdfExportIsError = true;
            PdfExportMessage = $"PDF konnte nicht erstellt werden: {ex.Message}";
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
