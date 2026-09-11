using MikrotikInstaller.App.ViewModels;
using Wpf.Ui.Controls;

namespace MikrotikInstaller.App;

public partial class MainWindow : FluentWindow
{
    public MainWindow()
    {
        InitializeComponent();
        Closing += OnClosing;
    }

    private async void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            await viewModel.DisposeSessionAsync();
        }
    }
}
