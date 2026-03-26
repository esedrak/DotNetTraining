# 🧪 HTTP Testing with WebApplicationFactory

`WebApplicationFactory<T>` is the C# equivalent of Go's `httptest` package. It spins up an in-memory test server with the full ASP.NET Core middleware pipeline — no port binding required.

---

## 1. Core Concepts

| Concept | Description |
| :--- | :--- |
| **`WebApplicationFactory<TEntryPoint>`** | Creates an in-memory test server for your ASP.NET Core app |
| **`CreateClient()`** | Returns an `HttpClient` pre-configured to call the test server |
| **`WithWebHostBuilder()`** | Override DI registrations for testing (swap real services for mocks) |
| **`IClassFixture<T>`** | Share the factory across all tests in a class (like `TestMain`) |

---

## 2. Go → C# Mapping

| Go | C# |
| :--- | :--- |
| `httptest.NewRecorder()` | `WebApplicationFactory.CreateClient()` |
| `httptest.NewServer(handler)` | `WebApplicationFactory<TEntryPoint>` |
| `ts.Client().Get(ts.URL + "/path")` | `client.GetAsync("/path")` |
| Swap handler for tests | `WithWebHostBuilder(b => b.ConfigureServices(...))` |

---

## 3. Example

```csharp
public class BankApiTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetAccount_Returns200_WhenExists()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/v1/accounts");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

### Override services for tests

```csharp
private WebApplicationFactory<Program> CreateFactory()
    => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
    {
        builder.ConfigureTestServices(services =>
        {
            // Replace real DB with in-memory fake
            services.RemoveAll<IBankRepository>();
            services.AddSingleton<IBankRepository, InMemoryBankRepository>();
        });
    });
```

---

## ⚠️ Pitfalls & Best Practices

1. Share the `WebApplicationFactory` via `IClassFixture<T>` — creating it per-test is expensive.
2. Use `ConfigureTestServices` (not `ConfigureServices`) to override only for tests.
3. Integration tests are slower than unit tests — prefer unit tests for business logic.
4. Your `Program.cs` must be accessible to the test project (`InternalsVisibleTo` or `public partial class Program`).

---

## 🏃 Running the Examples

```bash
dotnet test tests/Bank.Tests --filter "FullyQualifiedName~Integration"
```

---

## 📚 Further Reading

- [Integration tests in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)
- [WebApplicationFactory](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.testing.webapplicationfactory-1)
