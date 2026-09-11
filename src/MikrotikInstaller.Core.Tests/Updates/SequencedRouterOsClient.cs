using MikrotikInstaller.Core.Connectivity;

namespace MikrotikInstaller.Core.Tests.Updates;

/// <summary>Test-Double, das bei GetAsync nacheinander vorbereitete Antworten liefert (simuliert Polling).</summary>
internal sealed class SequencedRouterOsClient(Queue<IReadOnlyList<IReadOnlyDictionary<string, string>>> responses) : IRouterOsClient
{
    public int ExecuteCallCount { get; private set; }

    public RouterOsProtocol Protocol => RouterOsProtocol.Rest;

    public Task<IReadOnlyList<IReadOnlyDictionary<string, string>>> GetAsync(string path, CancellationToken cancellationToken = default) =>
        Task.FromResult(responses.Count > 0 ? responses.Dequeue() : []);

    public Task<string> AddAsync(string path, IReadOnlyDictionary<string, string> parameters, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task SetAsync(string path, string id, IReadOnlyDictionary<string, string> parameters, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task RemoveAsync(string path, string id, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task ExecuteAsync(string path, IReadOnlyDictionary<string, string>? parameters = null, CancellationToken cancellationToken = default)
    {
        ExecuteCallCount++;
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
