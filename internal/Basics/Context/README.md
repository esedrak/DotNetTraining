# 🚦 CancellationToken in C#

`CancellationToken` is C#'s equivalent of Go's `context.Context`. It propagates cancellation signals through call chains, enabling cooperative cancellation of async operations.

---

## 1. Core Concepts

| Concept | Description |
| :--- | :--- |
| **`CancellationToken`** | Passed to methods; checked to determine if cancellation was requested |
| **`CancellationTokenSource`** | Creates and controls a `CancellationToken`; call `.Cancel()` to signal |
| **`CancellationToken.None`** | Never-cancelled token (like `context.Background()`) |
| **Linked tokens** | Combine multiple sources with `CancellationTokenSource.CreateLinkedTokenSource()` |
| **Timeout** | `new CancellationTokenSource(TimeSpan)` (like `context.WithTimeout`) |
| **`OperationCanceledException`** | Thrown when an awaited operation is cancelled |
| **`AsyncLocal<T>`** | Ambient values scoped to an async flow (like `context.WithValue`) |

---

## 2. Go → C# Mapping

| Go | C# |
| :--- | :--- |
| `context.Background()` | `CancellationToken.None` |
| `context.WithCancel(ctx)` | `new CancellationTokenSource()` + `.Token` |
| `context.WithTimeout(ctx, d)` | `new CancellationTokenSource(timeSpan)` |
| `context.WithValue(ctx, k, v)` | `AsyncLocal<T>` or pass as explicit parameter |
| `ctx.Done()` channel | `token.WaitHandle` or `await Task.Delay(-1, token)` |
| `ctx.Err()` | `token.IsCancellationRequested` or catch `OperationCanceledException` |
| First parameter convention | Last parameter convention |

---

## 3. Key Difference from Go

> In Go, context is the **first** parameter. In C#, `CancellationToken` is the **last** parameter (by convention).

```go
// Go
func DoWork(ctx context.Context, id int) error { ... }
```

```csharp
// C#
async Task DoWorkAsync(int id, CancellationToken cancellationToken = default) { ... }
```

---

## 4. Implementation Examples

### Basic cancellation

```csharp
using var cts = new CancellationTokenSource();

// Cancel after 2 seconds
cts.CancelAfter(TimeSpan.FromSeconds(2));

try
{
    await DoWorkAsync(cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Cancelled!");
}
```

### Passing through a call chain

```csharp
async Task ProcessOrderAsync(int orderId, CancellationToken ct)
{
    var order = await FetchOrderAsync(orderId, ct);
    await ValidateAsync(order, ct);
    await SaveAsync(order, ct);
}
```

### Linked token sources (combine parent + local timeout)

```csharp
// Like context.WithTimeout on a parent context
using var cts = CancellationTokenSource.CreateLinkedTokenSource(parentToken);
cts.CancelAfter(TimeSpan.FromSeconds(5));
await DoWorkAsync(cts.Token);
```

---

## ⚠️ Pitfalls & Best Practices

> [!WARNING]
> Always propagate `CancellationToken` through your entire call chain. Swallowing it at a lower layer defeats the purpose.

1. Always use `catch (OperationCanceledException)` specifically — not `catch (Exception)` — to handle cancellation gracefully.
2. Use `token.ThrowIfCancellationRequested()` at checkpoints in CPU-bound loops.
3. `CancellationTokenSource` implements `IDisposable` — use `using var cts = ...`.
4. Default to `CancellationToken cancellationToken = default` in public APIs to make the parameter optional.

---

## 🏃 Running the Examples

```bash
dotnet test tests/Basics.Tests --filter "FullyQualifiedName~Context"
```

---

## 📚 Further Reading

- [Cancellation in managed threads](https://learn.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads)
- [Task cancellation](https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/task-cancellation)

## Your Next Step
Now that you've mastered the core features of C#, it's time to explore how to write more reusable, type-agnostic code.
Explore **[Generics](../Generics/README.md)** to see how to use type parameters to reduce code duplication.
