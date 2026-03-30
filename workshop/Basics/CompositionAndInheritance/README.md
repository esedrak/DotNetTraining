# 📦 Embedded Resources & Composition in C#

C# supports two complementary concepts covered here: **embedded resources** (compiling files into your assembly with `<EmbeddedResource>`) and **composition vs inheritance** (the two primary strategies for building type hierarchies and reusing behaviour).

---

## 1. Core Concepts

| Concept | Description |
| :--- | :--- |
| **`<EmbeddedResource>`** | Compile a file into the assembly at build time |
| **`GetManifestResourceStream()`** | Read an embedded file at runtime |
| **Composition** | Delegate to contained types (prefer over inheritance) |
| **Inheritance** | `class B : A` -- B is an A (use sparingly) |

---

## 2. Embedding Files

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

## 3. Composition vs Inheritance

```csharp
// Inheritance -- IS-A relationships
public class Animal { public string Name { get; init; } = ""; }
public class Dog : Animal { public void Bark() => Console.WriteLine("Woof!"); }

// Composition (preferred for HAS-A -- wrap an interface and delegate)
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

## Your Next Step
Now that you've mastered composition and embedded resources, it's time to learn how to organise your code into a clean and maintainable project structure.
Explore **[Package Layout](../Layout/README.md)** to understand the idiomatic way to structure .NET solutions.
