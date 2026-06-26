using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PostHogSample.Api.Options;

namespace PostHogSample.Api.Services;

public interface IPostHogAdminApiClient
{
    Task<JsonDocument> GetProjectAsync(string relativePath, CancellationToken cancellationToken = default);
    Task<JsonDocument> PostProjectAsync(string relativePath, object body, CancellationToken cancellationToken = default);
    Task<JsonDocument> GetEnvironmentAsync(string relativePath, CancellationToken cancellationToken = default);
    Task<JsonDocument> PostEnvironmentAsync(string relativePath, object body, CancellationToken cancellationToken = default);
    bool IsConfigured { get; }
}

/// <summary>
/// Thin wrapper around PostHog's private REST API for server-side analytics management.
/// </summary>
public sealed class PostHogAdminApiClient : IPostHogAdminApiClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly HttpClient _httpClient;
    private readonly PostHogAdminApiOptions _options;
    private readonly ILogger<PostHogAdminApiClient> _logger;

    public PostHogAdminApiClient(
        HttpClient httpClient,
        IOptions<PostHogAdminApiOptions> options,
        ILogger<PostHogAdminApiClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_options.PersonalApiKey)
        && !string.IsNullOrWhiteSpace(_options.ProjectId);

    public Task<JsonDocument> GetProjectAsync(string relativePath, CancellationToken cancellationToken = default)
        => SendAsync(HttpMethod.Get, "projects", relativePath, body: null, cancellationToken);

    public Task<JsonDocument> PostProjectAsync(string relativePath, object body, CancellationToken cancellationToken = default)
        => SendAsync(HttpMethod.Post, "projects", relativePath, body, cancellationToken);

    public Task<JsonDocument> GetEnvironmentAsync(string relativePath, CancellationToken cancellationToken = default)
        => SendAsync(HttpMethod.Get, "environments", relativePath, body: null, cancellationToken);

    public Task<JsonDocument> PostEnvironmentAsync(string relativePath, object body, CancellationToken cancellationToken = default)
        => SendAsync(HttpMethod.Post, "environments", relativePath, body, cancellationToken);

    private async Task<JsonDocument> SendAsync(
        HttpMethod method,
        string scope,
        string relativePath,
        object? body,
        CancellationToken cancellationToken)
    {
        EnsureConfigured();

        var requestUri = BuildUri(scope, relativePath);
        using var request = new HttpRequestMessage(method, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.PersonalApiKey);

        if (body is not null)
        {
            var json = JsonSerializer.Serialize(body, SerializerOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        _logger.LogDebug("PostHog admin API {Method} {Uri}", method, requestUri);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new PostHogAdminApiException(
                (int)response.StatusCode,
                $"PostHog admin API call failed ({(int)response.StatusCode}): {content}");
        }

        return JsonDocument.Parse(string.IsNullOrWhiteSpace(content) ? "{}" : content);
    }

    private void EnsureConfigured()
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException(
                "PostHog admin API is not configured. Set PostHogAdminApi:ProjectId and " +
                "PostHogAdminApi:PersonalApiKey (or PostHog:PersonalApiKey) in configuration.");
        }
    }

    private string BuildUri(string scope, string relativePath)
    {
        var baseUrl = _options.AppHostUrl.TrimEnd('/');
        var projectId = _options.ProjectId.Trim('/');
        var path = relativePath.TrimStart('/');

        return $"{baseUrl}/api/{scope}/{projectId}/{path}";
    }
}

public sealed class PostHogAdminApiException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
