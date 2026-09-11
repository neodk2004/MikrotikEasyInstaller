using System.Windows;
using System.Windows.Controls;

namespace MikrotikInstaller.App.Views.Controls;

/// <summary>Kleines "?"-Symbol mit Tooltip, um Fachbegriffe im Assistenten laientauglich zu erklären.</summary>
public partial class InfoHint : UserControl
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(InfoHint), new PropertyMetadata(string.Empty));

    public InfoHint()
    {
        InitializeComponent();
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
}
