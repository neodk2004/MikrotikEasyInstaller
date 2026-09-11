using MikrotikInstaller.App.ViewModels;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace MikrotikInstaller.App;

public partial class MainWindow : FluentWindow
{
    public MainWindow()
    {
        InitializeComponent();
        Closing += OnClosing;

        // Folgt Hell/Dunkel-Wechseln des Windows-Systemthemes zur Laufzeit;
        // updateAccents:false, damit unser MikroTik-Grün-Akzent dabei erhalten bleibt.
        SystemThemeWatcher.Watch(this, WindowBackdropType.Mica, updateAccents: false);
    }

    private async void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            await viewModel.DisposeSessionAsync();
        }
    }
}
