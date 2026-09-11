namespace MikrotikInstaller.Core.Connectivity.Demo;

/// <summary>
/// Simulierter RouterOS-Client für interne Tests des Assistenten, ganz ohne echtes Gerät.
/// Wird ausschließlich aktiviert, wenn exakt die Test-Zugangsdaten verwendet werden
/// (<see cref="Matches"/>) — es findet dabei keinerlei Netzwerkverbindung statt, alle Daten
/// liegen nur im Arbeitsspeicher dieses Objekts. Nur in Debug-Builds erreichbar
/// (siehe <see cref="RouterOsClientFactory"/>), damit dieser Testzugang nicht in der
/// veröffentlichten Version landet.
/// </summary>
public sealed class DemoRouterOsClient : IRouterOsClient
{
    public const string DemoHost = "127.127.127.127";
    public const string DemoUsername = "admin";
    public const string DemoPassword = "admin";

    private readonly Dictionary<string, List<Dictionary<string, string>>> _data = BuildInitialData();
    private int _nextId = 1;

    public static bool Matches(RouterOsCredentials credentials) =>
        credentials.Host == DemoHost && credentials.Username == DemoUsername && credentials.Password == DemoPassword;

    public RouterOsProtocol Protocol => RouterOsProtocol.Rest;

    public Task<IReadOnlyList<IReadOnlyDictionary<string, string>>> GetAsync(string path, CancellationToken cancellationToken = default)
    {
        var rows = _data.TryGetValue(Normalize(path), out var list) ? list : [];
        IReadOnlyList<IReadOnlyDictionary<string, string>> result = rows.Select(r => (IReadOnlyDictionary<string, string>)r).ToList();
        return Task.FromResult(result);
    }

    public Task<string> AddAsync(string path, IReadOnlyDictionary<string, string> parameters, CancellationToken cancellationToken = default)
    {
        var id = $"*{_nextId++}";
        var row = new Dictionary<string, string>(parameters) { [".id"] = id };

        var key = Normalize(path);
        if (!_data.TryGetValue(key, out var list))
        {
            list = [];
            _data[key] = list;
        }

        list.Add(row);
        return Task.FromResult(id);
    }

    public Task SetAsync(string path, string id, IReadOnlyDictionary<string, string> parameters, CancellationToken cancellationToken = default)
    {
        if (_data.TryGetValue(Normalize(path), out var list))
        {
            var row = list.FirstOrDefault(r => r.GetValueOrDefault(".id") == id);
            if (row is not null)
            {
                foreach (var (key, value) in parameters)
                {
                    row[key] = value;
                }
            }
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string path, string id, CancellationToken cancellationToken = default)
    {
        if (_data.TryGetValue(Normalize(path), out var list))
        {
            list.RemoveAll(r => r.GetValueOrDefault(".id") == id);
        }

        return Task.CompletedTask;
    }

    public Task ExecuteAsync(string path, IReadOnlyDictionary<string, string>? parameters = null, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private static string Normalize(string path) => path.TrimEnd('/');

    private static Dictionary<string, List<Dictionary<string, string>>> BuildInitialData() => new()
    {
        ["/system/resource"] = [new() { ["board-name"] = "CHR (Simulator)", ["version"] = "7.15.2 (stable)", ["architecture-name"] = "x86_64" }],
        ["/system/identity"] = [new() { ["name"] = "Test-Router" }],
        ["/system/routerboard"] = [new() { ["routerboard"] = "no" }],
        ["/interface"] =
        [
            new() { [".id"] = "*1", ["name"] = "ether1", ["type"] = "ether", ["running"] = "true", ["disabled"] = "false" },
            new() { [".id"] = "*2", ["name"] = "ether2", ["type"] = "ether", ["running"] = "true", ["disabled"] = "false" },
            new() { [".id"] = "*3", ["name"] = "ether3", ["type"] = "ether", ["running"] = "false", ["disabled"] = "false" },
            new() { [".id"] = "*4", ["name"] = "wlan1", ["type"] = "wlan", ["running"] = "false", ["disabled"] = "true" },
        ],
        ["/system/package/update"] =
        [
            new() { ["installed-version"] = "7.15.2", ["latest-version"] = "7.16.1", ["status"] = "New version is available" },
        ],
    };
}
