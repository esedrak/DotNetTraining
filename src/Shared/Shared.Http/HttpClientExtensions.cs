using System.Net.Http.Json;
using System.Text.Json;

namespace Shared.Http;

/// <summary>
/// Extension methods on <see cref="HttpClient"/> for typed JSON requests.
/// Wraps <c>System.Net.Http.Json</c> with consistent serializer options
/// and convenience overloads.
/// </summary>
public static class HttpClientExtensions
{
    private static readonly JsonSerializerOptions DefaultOptions =
        new(JsonSerializerDefaults.Web);   // camelCase, case-insensitive

    // ── GET ───────────────────────────────────────────────────────────────────

    /// <summary>GET <paramref name="requestUri"/> and deserialise the JSON body as <typeparamref name="T"/>.</summary>
    public static async Task<T?> GetJsonAsync<T>(
        this HttpClient client,
        string requestUri,
        CancellationToken ct = default)
    {
        var response = await client.GetAsync(requestUri, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(DefaultOptions, ct);
    }

    /// <summary>
    /// Like <see cref="GetJsonAsync{T}"/> but returns a success flag instead of
    /// throwing on 4xx — useful for optional lookups.
    /// </summary>
    public static async Task<(bool IsSuccess, T? Value)> TryGetJsonAsync<T>(
        this HttpClient client,
        string requestUri,
        CancellationToken ct = default)
    {
        var response = await client.GetAsync(requestUri, ct);
        if (!response.IsSuccessStatusCode)
        {
            return (false, default);
        }

        var value = await response.Content.ReadFromJsonAsync<T>(DefaultOptions, ct);
        return (true, value);
    }

    // ── POST ──────────────────────────────────────────────────────────────────

    /// <summary>POST <paramref name="body"/> as JSON and deserialise the response as <typeparamref name="TResponse"/>.</summary>
    public static async Task<TResponse?> PostJsonAsync<TRequest, TResponse>(
        this HttpClient client,
        string requestUri,
        TRequest body,
        CancellationToken ct = default)
    {
        var response = await client.PostAsJsonAsync(requestUri, body, DefaultOptions, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(DefaultOptions, ct);
    }
}
