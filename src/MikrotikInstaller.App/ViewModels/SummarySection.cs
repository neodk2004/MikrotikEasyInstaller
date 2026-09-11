using Wpf.Ui.Controls;

namespace MikrotikInstaller.App.ViewModels;

/// <summary>Ein Themenbereich in der Zusammenfassung (z. B. "Internet-Zugang") mit laientauglichen Kurzsätzen.</summary>
public sealed record SummarySection(string Title, SymbolRegular Icon, IReadOnlyList<string> Lines);
