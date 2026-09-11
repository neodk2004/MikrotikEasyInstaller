using MikrotikInstaller.Core.Connectivity;

namespace MikrotikInstaller.Core.Configuration;

/// <summary>
/// Eine einzelne, für sich verständliche Änderung am Gerät: ein laientauglicher Beschreibungstext
/// für die Zusammenfassung im Assistenten, und die Aktion, die sie tatsächlich anwendet.
/// Nichts wird ausgeführt, bevor der Nutzer im Zusammenfassungs-Schritt zugestimmt hat.
/// </summary>
public sealed record ConfigurationAction(string Description, Func<IRouterOsClient, CancellationToken, Task> ApplyAsync);
