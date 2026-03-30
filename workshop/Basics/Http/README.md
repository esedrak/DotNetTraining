# HTTP in C# — HttpClient & Minimal APIs

ASP.NET Core Minimal APIs provide a lightweight, high-performance way to build HTTP servers with concise route handlers like `app.MapGet(...)`. On the client side, `HttpClient` (managed through `IHttpClientFactory`) handles outbound HTTP requests with built-in connection pooling and lifetime management.

---

## 1. Core Concepts

| Concept | Description |
| :--- | :--- |
| **`HttpClient`** | HTTP client; always use via `IHttpClientFactory` in production |
| **`IHttpClientFactory`** | Manages `HttpClient` lifetime — prevents socket exhaustion |
| **Minimal APIs** | `app.MapGet(...)`, `app.MapPost(...)` — concise route handlers |
| **`System.Text.Json`** | Built-in JSON serialization and deserialization |
| **`HttpResponseMessage`** | Represents an HTTP response |

---

## 2. Examples

### Minimal API (server)

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/hello", () => "Hello, World!");

app.MapGet("/accounts/{id:guid}", async (Guid id, IBankService svc) =>
{
    var account = await svc.GetAccountAsync(id);
    return account is null ? Results.NotFound() : Results.Ok(account);
});

app.MapPost("/accounts", async (CreateAccountRequest req, IBankService svc) =>
{
    var account = await svc.CreateAccountAsync(req.Owner, req.InitialBalance);
    return Results.Created($"/accounts/{account.Id}", account);
});

app.Run();
```

### HttpClient (typed client)

```csharp
public class BankApiClient(HttpClient client)
{
    public async Task<Account?> GetAccountAsync(Guid id, CancellationToken ct = default)
    {
        var response = await client.GetAsync($"accounts/{id}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Account>(ct);
    }
}

// Register in DI
builder.Services.AddHttpClient<BankApiClient>(c =>
    c.BaseAddress = new Uri("https://api.example.com/v1/"));
```

---

## 3. Pitfalls & Best Practices

1. Never `new HttpClient()` directly in production — use `IHttpClientFactory` to avoid socket exhaustion.
2. Always call `response.EnsureSuccessStatusCode()` or check `response.IsSuccessStatusCode` — don't assume success.
3. `HttpClient` is thread-safe and meant to be reused — don't dispose it after each request.
4. Use `CancellationToken` everywhere in HTTP calls — network requests must be cancellable.

---

## 4. Running the Examples

```bash
dotnet test tests/Basics.Tests --filter "FullyQualifiedName~Http"
```

---

## 5. Further Reading

- [HttpClient guidelines](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines)
- [Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [IHttpClientFactory](https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory)

---

## Your Next Step
With your HTTP services built, you need a way to test them efficiently without relying on an actual network.
Explore **[HTTP Testing](../HttpTest/README.md)** to learn how to test handlers and clients in isolation using `WebApplicationFactory<T>`.
