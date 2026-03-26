# 📦 Embedded Resources & Composition in C#

Go has `//go:embed` for embedding files at compile time and struct embedding for composition. C#'s equivalent for files is `<EmbeddedResource>` in the `.csproj`, and for composition it uses inheritance or explicit delegation.

---

## 1. Core Concepts

| Concept | Description |
| :--- | :--- |
| **`<EmbeddedResource>`** | Compile a file into the assembly — equivalent to `//go:embed` |
| **`GetManifestResourceStream()`** | Read an embedded file at runtime |
| **Composition** | Delegate to contained types (prefer over inheritance) |
| **Inheritance** | `class B : A` — B is an A (use sparingly) |

---

## 2. Go → C# Mapping

| Go | C# |
| :--- | :--- |
| `//go:embed file.txt` | `<EmbeddedResource Include="file.txt" />` in `.csproj` |
| `embed.FS` | `Assembly.GetManifestResourceStream()` |
| Struct embedding `type B struct { A }` | Inheritance `class B : A` or composition field |
| Promoted methods | Inherited/delegated methods |

---

## 3. Embedding Files

### In `.csproj`

```xml
<ItemGroup>
  <EmbeddedResource Include="Data/seed.json" />
  <EmbeddedResource Include="Templates/**/*.html" />
</ItemGroup>
```

### Reading at runtime

```csharp
var assembly = Assembly.GetExecutingAssembly();
var resourceName = "MyProject.Data.seed.json";

using var stream = assembly.GetManifestResourceStream(resourceName)!;
using var reader = new StreamReader(stream);
var json = await reader.ReadToEndAsync();
```

### Using `EmbeddedResource` attribute helper (simpler)

```csharp
// Modern approach: use the auto-generated resource accessor
// (generated via <EmbeddedResource> + MSBuild source generators)
var content = Properties.Resources.seed_json;
```

---

## 4. Composition vs Inheritance

```csharp
// Inheritance (Go struct embedding equivalent for IS-A relationships)
public class Animal { public string Name { get; init; } = ""; }
public class Dog : Animal { public void Bark() => Console.WriteLine("Woof!"); }

// Composition (preferred for HAS-A — like wrapping an interface)
public class LoggingRepository(IBankRepository inner, ILogger<LoggingRepository> logger)
    : IBankRepository
{
    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        logger.LogInformation("Getting account {Id}", id);
        return await inner.GetByIdAsync(id, ct);
    }
    // ... other methods delegate to inner
}
```

---

## ⚠️ Pitfalls & Best Practices

1. Prefer **composition over inheritance** — it's more flexible and testable.
2. Resource names in `GetManifestResourceStream()` use dots as separators and include the default namespace prefix.
3. Use `typeof(T).Assembly` instead of `Assembly.GetExecutingAssembly()` for libraries.

---

## 📚 Further Reading

- [Embedded resources](https://learn.microsoft.com/en-us/dotnet/core/extensions/create-resource-files)
- [Composition vs Inheritance](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/architectural-principles)
