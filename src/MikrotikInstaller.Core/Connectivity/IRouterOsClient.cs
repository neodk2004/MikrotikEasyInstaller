namespace MikrotikInstaller.Core.Connectivity;

/// <summary>
/// Einheitliche Schnittstelle zu einem RouterOS-Gerät, unabhängig davon, ob im Hintergrund die REST-API
/// oder die binäre API verwendet wird. Pfade folgen der RouterOS-Menüstruktur, z. B. "/ip/address",
/// "/interface/vlan", "/ip/firewall/filter".
/// </summary>
public interface IRouterOsClient : IAsyncDisposable
{
    RouterOsProtocol Protocol { get; }

    /// <summary>Listet alle Einträge unter dem angegebenen Menüpfad auf (entspricht RouterOS "print").</summary>
    Task<IReadOnlyList<IReadOnlyDictionary<string, string>>> GetAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>Legt einen neuen Eintrag an (entspricht RouterOS "add") und liefert dessen ID zurück.</summary>
    Task<string> AddAsync(string path, IReadOnlyDictionary<string, string> parameters, CancellationToken cancellationToken = default);

    /// <summary>Ändert einen bestehenden Eintrag anhand seiner ID (entspricht RouterOS "set").</summary>
    Task SetAsync(string path, string id, IReadOnlyDictionary<string, string> parameters, CancellationToken cancellationToken = default);

    /// <summary>Entfernt einen Eintrag anhand seiner ID (entspricht RouterOS "remove").</summary>
    Task RemoveAsync(string path, string id, CancellationToken cancellationToken = default);

    /// <summary>Führt einen eigenständigen Befehl aus, der keinem Listen-Eintrag entspricht (z. B. "/system/backup/save").</summary>
    Task ExecuteAsync(string path, IReadOnlyDictionary<string, string>? parameters = null, CancellationToken cancellationToken = default);
}
