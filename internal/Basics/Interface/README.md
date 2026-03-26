# 🔌 Interfaces in C#

C# interfaces are **explicitly declared** and **explicitly implemented**, unlike Go's implicit structural typing. The key mindset shift: in C#, you declare conformance with `: IMyInterface`, while in Go, a type satisfies an interface automatically.

---

## 1. Core Concepts

| Concept | Description |
| :--- | :--- |
| **`interface`** | Declares a contract; no implementation (unless using default methods) |
| **Explicit implementation** | Type declares it satisfies the interface with `: IInterface` |
| **DI / IoC** | Register interfaces in the DI container; inject via constructor |
| **Interface inheritance** | Interfaces can extend other interfaces |
| **Default methods (C# 8+)** | Interfaces can provide default implementations |
| **`IDisposable`** | Standard interface for resource cleanup (`using` statement) |

---

## 2. Go → C# Mapping

| Go | C# |
| :--- | :--- |
| Implicit satisfaction | `: IInterface` declaration required |
| Consumer-side definition | Same pattern works (define interface near consumer) |
| Interface composition | Interface inheritance: `interface IA : IB, IC` |
| Empty interface `any` | `object` or `T` (generic) |
| N/A | Default interface methods |
| N/A | DI container registration |

---

## 3. Implementation Examples

### Define and implement an interface

```csharp
public interface IGreeter
{
    string Greet(string name);
}

public class FormalGreeter : IGreeter
{
    public string Greet(string name) => $"Good day, {name}.";
}

public class CasualGreeter : IGreeter
{
    public string Greet(string name) => $"Hey {name}!";
}
```

### Program to the interface (DI-ready)

```csharp
// Consumer only knows about IGreeter — not the concrete type
public class WelcomeService(IGreeter greeter)
{
    public string Welcome(string name) => greeter.Greet(name);
}
```

### Register in DI container (ASP.NET Core / Generic Host)

```csharp
builder.Services.AddScoped<IGreeter, FormalGreeter>();
builder.Services.AddScoped<WelcomeService>();
```

### Interface inheritance

```csharp
public interface IReadRepository<T>
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
}

public interface IWriteRepository<T> : IReadRepository<T>
{
    Task AddAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
```

---

## 4. Common Patterns

- **Repository pattern**: `IRepository<T>` with EF Core implementation → easy to mock in tests
- **Strategy pattern**: Swap implementations at runtime via DI
- **Decorator pattern**: Wrap an interface to add cross-cutting concerns (logging, caching)

---

## ⚠️ Pitfalls & Best Practices

1. Keep interfaces focused (Interface Segregation Principle — ISP from SOLID).
2. Prefer constructor injection over `IServiceLocator.GetService<T>()` (anti-pattern).
3. Don't interface everything — only what you need to mock or swap.
4. `IDisposable` is a special interface: always implement it when managing unmanaged resources.

---

## 🏃 Running the Examples

```bash
dotnet test tests/Basics.Tests --filter "FullyQualifiedName~Interface"
```

---

## 📚 Further Reading

- [Interfaces (C# docs)](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/interfaces)
- [Dependency injection in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
