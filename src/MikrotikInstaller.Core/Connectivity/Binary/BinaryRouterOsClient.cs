using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;

namespace MikrotikInstaller.Core.Connectivity.Binary;

/// <summary>
/// Client für die binäre RouterOS-API (Port 8728/8729). Fallback für Geräte ohne REST-API
/// (RouterOS älter als Version 7). Verwendet den seit RouterOS 6.43 gültigen einstufigen
/// Login (Name+Passwort in einem Satz, keine MD5-Challenge mehr) — ältere Geräte werden
/// dadurch nicht unterstützt, sind aber seit 2018 End-of-Life.
/// </summary>
public sealed class BinaryRouterOsClient : IRouterOsClient
{
    private readonly TcpClient _tcpClient;
    private readonly Stream _stream;

    public RouterOsProtocol Protocol => RouterOsProtocol.Binary;

    private BinaryRouterOsClient(TcpClient tcpClient, Stream stream)
    {
        _tcpClient = tcpClient;
        _stream = stream;
    }

    public static async Task<BinaryRouterOsClient> ConnectAsync(RouterOsCredentials credentials, CancellationToken cancellationToken = default)
    {
        var port = credentials.BinaryPort ?? (credentials.UseEncryption ? 8729 : 8728);

        var tcpClient = new TcpClient();
        try
        {
            await tcpClient.ConnectAsync(credentials.Host, port, cancellationToken);
        }
        catch (Exception ex)
        {
            tcpClient.Dispose();
            throw new RouterOsException($"Das Gerät unter {credentials.Host}:{port} ist nicht erreichbar.", ex);
        }

        Stream stream = tcpClient.GetStream();

        if (credentials.UseEncryption)
        {
            var sslStream = new SslStream(
                stream,
                leaveInnerStreamOpen: false,
                userCertificateValidationCallback: (_, _, _, _) => credentials.AllowUntrustedCertificate);

            try
            {
                await sslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
                {
                    TargetHost = credentials.Host,
                    EnabledSslProtocols = SslProtocols.None,
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                tcpClient.Dispose();
                throw new RouterOsException("Die verschlüsselte Verbindung zum Gerät konnte nicht aufgebaut werden.", ex);
            }

            stream = sslStream;
        }

        var client = new BinaryRouterOsClient(tcpClient, stream);
        try
        {
            await client.LoginAsync(credentials.Username, credentials.Password, cancellationToken);
        }
        catch
        {
            await client.DisposeAsync();
            throw;
        }

        return client;
    }

    private async Task LoginAsync(string username, string password, CancellationToken cancellationToken)
    {
        await RouterOsWireFormat.WriteSentenceAsync(
            _stream,
            [
                "/login",
                $"=name={username}",
                $"=password={password}",
            ],
            cancellationToken);

        var reply = await ReadReplyAsync(cancellationToken);
        if (reply.Status == RouterOsReplyStatus.Trap)
        {
            throw new RouterOsException("Benutzername oder Passwort ist falsch.");
        }
    }

    public async Task<IReadOnlyList<IReadOnlyDictionary<string, string>>> GetAsync(string path, CancellationToken cancellationToken = default)
    {
        await RouterOsWireFormat.WriteSentenceAsync(_stream, [NormalizeCommand(path, "print")], cancellationToken);

        var rows = new List<IReadOnlyDictionary<string, string>>();
        while (true)
        {
            var reply = await ReadReplyAsync(cancellationToken);
            switch (reply.Status)
            {
                case RouterOsReplyStatus.Row:
                    rows.Add(reply.Attributes);
                    break;
                case RouterOsReplyStatus.Done:
                    return rows;
                case RouterOsReplyStatus.Trap:
                    throw new RouterOsException(DescribeError(reply));
            }
        }
    }

    public async Task<string> AddAsync(string path, IReadOnlyDictionary<string, string> parameters, CancellationToken cancellationToken = default)
    {
        var words = new List<string> { NormalizeCommand(path, "add") };
        words.AddRange(parameters.Select(kv => $"={kv.Key}={kv.Value}"));

        await RouterOsWireFormat.WriteSentenceAsync(_stream, words, cancellationToken);
        var reply = await ReadReplyAsync(cancellationToken);
        if (reply.Status == RouterOsReplyStatus.Trap)
        {
            throw new RouterOsException(DescribeError(reply));
        }

        return reply.Attributes.GetValueOrDefault("ret", string.Empty);
    }

    public async Task SetAsync(string path, string id, IReadOnlyDictionary<string, string> parameters, CancellationToken cancellationToken = default)
    {
        var words = new List<string> { NormalizeCommand(path, "set"), $"=.id={id}" };
        words.AddRange(parameters.Select(kv => $"={kv.Key}={kv.Value}"));

        await RouterOsWireFormat.WriteSentenceAsync(_stream, words, cancellationToken);
        var reply = await ReadReplyAsync(cancellationToken);
        if (reply.Status == RouterOsReplyStatus.Trap)
        {
            throw new RouterOsException(DescribeError(reply));
        }
    }

    public async Task RemoveAsync(string path, string id, CancellationToken cancellationToken = default)
    {
        await RouterOsWireFormat.WriteSentenceAsync(
            _stream,
            [NormalizeCommand(path, "remove"), $"=.id={id}"],
            cancellationToken);

        var reply = await ReadReplyAsync(cancellationToken);
        if (reply.Status == RouterOsReplyStatus.Trap)
        {
            throw new RouterOsException(DescribeError(reply));
        }
    }

    public async Task ExecuteAsync(string path, IReadOnlyDictionary<string, string>? parameters = null, CancellationToken cancellationToken = default)
    {
        var words = new List<string> { path.StartsWith('/') ? path : "/" + path };
        if (parameters is not null)
        {
            words.AddRange(parameters.Select(kv => $"={kv.Key}={kv.Value}"));
        }

        await RouterOsWireFormat.WriteSentenceAsync(_stream, words, cancellationToken);

        while (true)
        {
            var reply = await ReadReplyAsync(cancellationToken);
            if (reply.Status == RouterOsReplyStatus.Trap)
            {
                throw new RouterOsException(DescribeError(reply));
            }

            if (reply.Status == RouterOsReplyStatus.Done)
            {
                return;
            }
        }
    }

    private static string NormalizeCommand(string path, string action)
    {
        var trimmed = path.TrimEnd('/');
        if (!trimmed.StartsWith('/'))
        {
            trimmed = "/" + trimmed;
        }

        return $"{trimmed}/{action}";
    }

    private async Task<RouterOsReply> ReadReplyAsync(CancellationToken cancellationToken)
    {
        var words = await RouterOsWireFormat.ReadSentenceAsync(_stream, cancellationToken);
        var attributes = new Dictionary<string, string>();

        foreach (var word in words.Skip(1))
        {
            if (word.Length == 0 || word[0] != '=')
            {
                continue;
            }

            var withoutPrefix = word[1..];
            var separatorIndex = withoutPrefix.IndexOf('=');
            if (separatorIndex >= 0)
            {
                attributes[withoutPrefix[..separatorIndex]] = withoutPrefix[(separatorIndex + 1)..];
            }
        }

        var replyWord = words.Count > 0 ? words[0] : string.Empty;
        var status = replyWord switch
        {
            "!re" => RouterOsReplyStatus.Row,
            "!trap" => RouterOsReplyStatus.Trap,
            "!fatal" => RouterOsReplyStatus.Trap,
            _ => RouterOsReplyStatus.Done,
        };

        return new RouterOsReply(status, attributes);
    }

    private static string DescribeError(RouterOsReply reply) =>
        reply.Attributes.TryGetValue("message", out var message)
            ? message
            : "Das Gerät hat die Anfrage abgelehnt.";

    public ValueTask DisposeAsync()
    {
        _stream.Dispose();
        _tcpClient.Dispose();
        return ValueTask.CompletedTask;
    }
}
