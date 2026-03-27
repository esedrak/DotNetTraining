using System.Net.Http.Json;
using System.Text.Json;

namespace DotNetTraining.Basics.Http;

// ── Typed HttpClient — replaces Go's &http.Client{} ──────────────────────────

/// <summary>
/// Typed HttpClient pattern: inject HttpClient via constructor, register with IHttpClientFactory.
/// This avoids socket exhaustion from creating new HttpClient instances.
/// Equivalent to Go's http.Client with a base URL.
/// </summary>
public class BankApiClient(HttpClient client)
{
    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNameCaseInsensitive = true };

    // GET /accounts — returns null if 404, throws on other errors
    public async Task<List<AccountDto>?> GetAccountsAsync(CancellationToken ct = default)
    {
        var response = await client.GetAsync("/v1/accounts", ct);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<AccountDto>>(JsonOptions, ct);
    }

    // GET /accounts/{id}
    public async Task<AccountDto?> GetAccountAsync(Guid id, CancellationToken ct = default)
    {
        var response = await client.GetAsync($"/v1/accounts/{id}", ct);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AccountDto>(JsonOptions, ct);
    }

    // POST /accounts
    public async Task<AccountDto> CreateAccountAsync(
        string owner, decimal initialBalance = 0m, CancellationToken ct = default)
    {
        var response = await client.PostAsJsonAsync(
            "/v1/accounts", new { owner, initialBalance }, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AccountDto>(JsonOptions, ct))!;
    }
}

public record AccountDto(Guid Id, string Owner, decimal Balance);

// ── HttpClientFactory helpers — show DI registration pattern ─────────────────

/// <summary>
/// Shows how to register typed clients in DI — call this in Program.cs.
/// Equivalent to Go's: client := &http.Client{Timeout: 10 * time.Second}
/// </summary>
public static class HttpRegistration
{
    /// <summary>
    /// builder.Services.AddBankApiClient("https://api.example.com/");
    /// </summary>
    public static Microsoft.Extensions.DependencyInjection.IServiceCollection AddBankApiClient(
        this Microsoft.Extensions.DependencyInjection.IServiceCollection services,
        string baseUrl)
    {
        services.AddHttpClient<BankApiClient>(c =>
        {
            c.BaseAddress = new Uri(baseUrl);
            c.Timeout = TimeSpan.FromSeconds(10);
            c.DefaultRequestHeaders.Add("Accept", "application/json");
        });
        return services;
    }
}

// ── Request builder pattern — fluent HTTP request construction ────────────────

/// <summary>
/// Demonstrates building HTTP requests manually — useful for custom headers,
/// multipart, or non-JSON bodies.
/// </summary>
public static class HttpRequestExamples
{
    public static HttpRequestMessage BuildAuthenticatedRequest(
        string path, string bearerToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
        return request;
    }

    public static async Task<string> SendWithRetryAsync(
        HttpClient client, string path, int maxAttempts = 3,
        CancellationToken ct = default)
    {
        Exception? lastEx = null;
        for (int i = 0; i < maxAttempts; i++)
        {
            try
            {
                var response = await client.GetAsync(path, ct);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync(ct);
            }
            catch (HttpRequestException ex) when (i < maxAttempts - 1)
            {
                lastEx = ex;
                await Task.Delay(TimeSpan.FromMilliseconds(100 * (i + 1)), ct);
            }
        }
        throw lastEx!;
    }
}
