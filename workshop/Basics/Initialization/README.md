# Static Constructors & Module Initializers in C#

C# provides two mechanisms for running initialization code automatically: **static constructors** (once per class, before first use) and **`[ModuleInitializer]`** (once per assembly at load time, .NET 5+). These let you set up configuration, register services, or establish invariants before any other code runs.

---

## 1. Core Concepts

| Concept | Description |
| :--- | :--- |
| **Static constructor** | Runs once, before the first use of the class; no parameters |
| **`[ModuleInitializer]`** | Runs once when the assembly loads; assembly-level initialization |
| **Static field initializer** | Runs before the static constructor |
| **Execution order** | Static field initializers → static constructor → first instance creation |

---

## 2. Examples

### Static constructor

```csharp
public class Config
{
    public static readonly string ConnectionString;
    public static readonly int MaxRetries;

    // Called once, automatically, before any static member is accessed
    static Config()
    {
        ConnectionString = Environment.GetEnvironmentVariable("DB_URL")
            ?? "Host=localhost;Database=dotnetbank";
        MaxRetries = int.TryParse(
            Environment.GetEnvironmentVariable("MAX_RETRIES"), out int r) ? r : 3;
    }
}
```

### `[ModuleInitializer]` — assembly-level init

```csharp
internal static class Startup
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        // Runs once when the assembly loads — before any user code
        Console.WriteLine("Module initialized");
    }
}
```

---

## 3. Lazy Initialization with `Lazy<T>`

For expensive initialization that should be deferred until first access, use `Lazy<T>`:

```csharp
public class ServiceRegistry
{
    // Thread-safe by default; computed only when .Value is first accessed
    private static readonly Lazy<HttpClient> _client = new(() =>
    {
        var client = new HttpClient { BaseAddress = new Uri("https://api.example.com") };
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        return client;
    });

    public static HttpClient Client => _client.Value;
}
```

This is preferred over static constructors when the initialization is expensive and may not always be needed.

---

## Pitfalls & Best Practices

1. Static constructors cannot throw — an unhandled exception makes the class unusable for the lifetime of the AppDomain.
2. Don't use static constructors for heavy I/O — prefer lazy initialization (`Lazy<T>`) or DI.
3. `[ModuleInitializer]` must be on an `internal static void` method with no parameters.
4. Execution order between assemblies is not guaranteed — don't depend on cross-assembly init ordering.

---

## Running the Examples

```bash
dotnet test tests/Basics.Tests --filter "FullyQualifiedName~Init"
```

---

## Further Reading

- [Static constructors (C# docs)](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/static-constructors)
- [Module initializers](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/module-initializers)

---

## Your Next Step
Now that your application is starting up correctly, you need a robust way to verify it works through automated testing.
Explore **[Testing](../Testing/README.md)** to learn xUnit's idiomatic approach to unit and table-driven tests.
