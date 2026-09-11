using CommunityToolkit.Mvvm.ComponentModel;

namespace MikrotikInstaller.App.ViewModels.Steps;

/// <summary>Eine Schnittstelle mit an-/abwählbarem Kontrollkästchen, z. B. für die Heimnetz-Zuordnung.</summary>
public partial class SelectableInterface(string name, bool isSelected) : ObservableObject
{
    public string Name { get; } = name;

    [ObservableProperty]
    private bool _isSelected = isSelected;
}
