using CommunityToolkit.Mvvm.ComponentModel;

namespace MikrotikInstaller.App.ViewModels.Steps;

/// <summary>Eine Zeile im VLAN-Schritt: Eingaben für ein zusätzliches, getrenntes Netzwerk.</summary>
public partial class VlanEntry : ObservableObject
{
    [ObservableProperty]
    private string _vlanId = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _routerAddress = string.Empty;

    [ObservableProperty]
    private string _poolStart = string.Empty;

    [ObservableProperty]
    private string _poolEnd = string.Empty;
}
