using CommunityToolkit.Mvvm.ComponentModel;

namespace MikrotikInstaller.App.ViewModels.Steps;

/// <summary>Eine Zeile im Firewall-Schritt: Eingaben für eine optionale Portfreigabe.</summary>
public partial class PortForwardEntry : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _protocol = "tcp";

    [ObservableProperty]
    private string _externalPort = string.Empty;

    [ObservableProperty]
    private string _targetAddress = string.Empty;

    [ObservableProperty]
    private string _internalPort = string.Empty;
}
