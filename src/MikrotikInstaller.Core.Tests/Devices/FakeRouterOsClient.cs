using MikrotikInstaller.Core.Connectivity;

namespace MikrotikInstaller.Core.Tests.Devices;

/// <summary>Test-Double für <see cref="IRouterOsClient"/>: liefert vorbereitete Antworten je Pfad.</summary>
internal sealed class FakeRouterOsClient(
    IReadOnlyDictionary<string, IReadOnlyList<IReadOnlyDictionary<string, string>>> responses) : IRouterOsClient
{
    public RouterOsProtocol Protocol => RouterOsProtocol.Rest;

    public Task<IReadOnlyList<IReadOnlyDictionary<string, string>>> GetAsync(string path, CancellationToken cancellationToken = default) =>
        Task.FromResult(responses.TryGetValue(path, out var rows) ? rows : []);

    public Task<string> AddAsync(string path, IReadOnlyDictionary<string, string> parameters, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task SetAsync(string path, string id, IReadOnlyDictionary<string, string> parameters, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task RemoveAsync(string path, string id, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task ExecuteAsync(string path, IReadOnlyDictionary<string, string>? parameters = null, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
