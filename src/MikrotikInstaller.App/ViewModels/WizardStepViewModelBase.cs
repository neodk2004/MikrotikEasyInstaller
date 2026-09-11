using CommunityToolkit.Mvvm.ComponentModel;

namespace MikrotikInstaller.App.ViewModels;

/// <summary>
/// Basisklasse für jeden Schritt im Einrichtungs-Assistenten.
/// </summary>
public abstract partial class WizardStepViewModelBase : ObservableObject
{
    protected WizardStepViewModelBase(string title, string description)
    {
        Title = title;
        Description = description;
    }

    public string Title { get; }

    public string Description { get; }

    /// <summary>
    /// Bestimmt, ob der Nutzer von diesem Schritt aus weitergehen darf.
    /// Wird von abgeleiteten Schritten überschrieben, sobald echte Validierung existiert.
    /// </summary>
    public virtual bool CanGoNext => true;

    /// <summary>
    /// Wird aufgerufen, sobald dieser Schritt im Assistenten zum aktuellen Schritt wird.
    /// Schritte, die beim Betreten automatisch Daten laden müssen (z. B. Geräte-Erkennung),
    /// überschreiben das hier.
    /// </summary>
    public virtual Task OnActivatedAsync() => Task.CompletedTask;
}
