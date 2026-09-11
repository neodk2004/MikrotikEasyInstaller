namespace MikrotikInstaller.App.ViewModels;

/// <summary>Anzeige-Zustand eines Schritts in der Sidebar-Übersicht.</summary>
public sealed record StepNavItem(int Number, string Title, bool IsCurrent, bool IsCompleted);
