using MikrotikInstaller.Core.Connectivity;

namespace MikrotikInstaller.Core.Tests.Configuration;

/// <summary>Test-Double, das aufgezeichnete Add/Execute-Aufrufe zur Prüfung bereitstellt.</summary>
internal sealed class RecordingRouterOsClient : IRouterOsClient
{
    public List<(string Path, IReadOnlyDictionary<string, string> Parameters)> AddCalls { get; } = [];

    public List<(string Path, IReadOnlyDictionary<string, string>? Parameters)> ExecuteCalls { get; } = [];

    public RouterOsProtocol Protocol => RouterOsProtocol.Rest;

    public Task<IReadOnlyList<IReadOnlyDictionary<string, string>>> GetAsync(string path, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<IReadOnlyDictionary<string, string>>>([]);

    public Task<string> AddAsync(string path, IReadOnlyDictionary<string, string> parameters, CancellationToken cancellationToken = default)
    {
        AddCalls.Add((path, parameters));
        return Task.FromResult("*1");
    }

    public Task SetAsync(string path, string id, IReadOnlyDictionary<string, string> parameters, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task RemoveAsync(string path, string id, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task ExecuteAsync(string path, IReadOnlyDictionary<string, string>? parameters = null, CancellationToken cancellationToken = default)
    {
        ExecuteCalls.Add((path, parameters));
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
