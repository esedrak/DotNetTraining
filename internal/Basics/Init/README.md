# 🚀 Static Constructors & Module Initializers in C#

Go's `func init()` runs once per package, before `main`. C# has two equivalents: **static constructors** (per class) and **`[ModuleInitializer]`** (per assembly, .NET 5+).

---

## 1. Core Concepts

| Concept | Description |
| :--- | :--- |
| **Static constructor** | Runs once, before the first use of the class; no parameters |
| **`[ModuleInitializer]`** | Runs once when the assembly loads; like package-level `init()` |
| **Static field initializer** | Runs before the static constructor |
| **Execution order** | Static field initializers → static constructor → first instance creation |

---

## 2. Go → C# Mapping

| Go | C# |
| :--- | :--- |
| `func init()` | `static ClassName()` constructor |
| Package-level `init()` | `[ModuleInitializer]` on a static method |
| Called automatically | Called automatically — cannot be invoked manually |
| Multiple `init()` per package OK | One static constructor per class |
| `var x = computeX()` at package level | `static readonly T X = ComputeX()` |

---

## 3. Examples

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

## ⚠️ Pitfalls & Best Practices

1. Static constructors cannot throw — an unhandled exception makes the class unusable for the lifetime of the AppDomain.
2. Don't use static constructors for heavy I/O — prefer lazy initialization (`Lazy<T>`) or DI.
3. `[ModuleInitializer]` must be on an `internal static void` method with no parameters.
4. Execution order between assemblies is not guaranteed — don't depend on cross-assembly init ordering.

---

## 🏃 Running the Examples

```bash
dotnet test tests/Basics.Tests --filter "FullyQualifiedName~Init"
```

---

## 📚 Further Reading

- [Static constructors (C# docs)](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/static-constructors)
- [Module initializers](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/module-initializers)

## Your Next Step
Now that your application is starting up correctly, you need a robust way to verify it works through automated testing.
Explore **[Testing](../Testing/README.md)** to learn xUnit's idiomatic approach to unit and table-driven tests.
