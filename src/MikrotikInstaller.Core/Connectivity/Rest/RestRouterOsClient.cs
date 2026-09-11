using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace MikrotikInstaller.Core.Connectivity.Rest;

/// <summary>
/// Client für die RouterOS-7-REST-API (JSON über HTTP/HTTPS unter "/rest/..."). Wird von der
/// <see cref="RouterOsClientFactory"/> immer zuerst versucht.
/// </summary>
public sealed class RestRouterOsClient : IRouterOsClient
{
    private readonly HttpClient _httpClient;

    public RouterOsProtocol Protocol => RouterOsProtocol.Rest;

    private RestRouterOsClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public static async Task<RestRouterOsClient> ConnectAsync(RouterOsCredentials credentials, CancellationToken cancellationToken = default)
    {
        var handler = new HttpClientHandler();
        if (credentials.AllowUntrustedCertificate)
        {
            // Bewusst gelockert für selbstsignierte Gerätezertifikate — das UI muss dafür einen
            // Warnhinweis anzeigen, siehe RouterOsCredentials.AllowUntrustedCertificate.
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        }

        var scheme = credentials.UseEncryption ? "https" : "http";
        var port = credentials.RestPort ?? (credentials.UseEncryption ? 443 : 80);

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri($"{scheme}://{credentials.Host}:{port}/rest/"),
            Timeout = TimeSpan.FromSeconds(8),
        };
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{credentials.Username}:{credentials.Password}")));

        var client = new RestRouterOsClient(httpClient);
        try
        {
            // Dient gleichzeitig als Erreichbarkeits- und Login-Test.
            await client.GetAsync("/system/resource", cancellationToken);
        }
        catch (RouterOsException)
        {
            httpClient.Dispose();
            throw;
        }
        catch (Exception ex)
        {
            httpClient.Dispose();
            throw new RouterOsException("Über die REST-Schnittstelle konnte keine Verbindung hergestellt werden.", ex);
        }

        return client;
    }

    public async Task<IReadOnlyList<IReadOnlyDictionary<string, string>>> GetAsync(string path, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Get, path, body: null, cancellationToken);
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var results = new List<IReadOnlyDictionary<string, string>>();
        switch (document.RootElement.ValueKind)
        {
            case JsonValueKind.Array:
                foreach (var element in document.RootElement.EnumerateArray())
                {
                    results.Add(ToDictionary(element));
                }

                break;
            case JsonValueKind.Object:
                results.Add(ToDictionary(document.RootElement));
                break;
        }

        return results;
    }

    public async Task<string> AddAsync(string path, IReadOnlyDictionary<string, string> parameters, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Put, path, parameters, cancellationToken);
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        return document.RootElement.TryGetProperty(".id", out var id) ? id.GetString() ?? string.Empty : string.Empty;
    }

    public async Task SetAsync(string path, string id, IReadOnlyDictionary<string, string> parameters, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(new HttpMethod("PATCH"), $"{path.TrimEnd('/')}/{id}", parameters, cancellationToken);
    }

    public async Task RemoveAsync(string path, string id, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Delete, $"{path.TrimEnd('/')}/{id}", body: null, cancellationToken);
    }

    public async Task ExecuteAsync(string path, IReadOnlyDictionary<string, string>? parameters = null, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Post, path, parameters, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method, string path, IReadOnlyDictionary<string, string>? body, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(method, path.TrimStart('/'));
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new RouterOsException("Die Verbindung zum Gerät wurde unterbrochen.", ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            response.Dispose();
            throw new RouterOsException(DescribeError(response.StatusCode));
        }

        return response;
    }

    private static IReadOnlyDictionary<string, string> ToDictionary(JsonElement element)
    {
        var dict = new Dictionary<string, string>();
        foreach (var property in element.EnumerateObject())
        {
            dict[property.Name] = property.Value.ValueKind == JsonValueKind.String
                ? property.Value.GetString() ?? string.Empty
                : property.Value.ToString();
        }

        return dict;
    }

    private static string DescribeError(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => "Benutzername oder Passwort ist falsch.",
        HttpStatusCode.NotFound => "Diese Einstellung wird von deinem Gerät nicht unterstützt.",
        _ => "Das Gerät hat die Anfrage abgelehnt.",
    };

    public ValueTask DisposeAsync()
    {
        _httpClient.Dispose();
        return ValueTask.CompletedTask;
    }
}
