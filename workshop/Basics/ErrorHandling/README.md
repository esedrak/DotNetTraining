# ⚠️ Error Handling in C#

C# uses **exceptions** for error propagation -- a structured mechanism with `try`/`catch`/`finally`, custom exception types, and powerful filter expressions (`catch when`). For expected failures where throwing is too heavy, the **Result pattern** provides a lightweight alternative.

---

## 1. Core Concepts

| Concept | Description |
| :--- | :--- |
| **`Exception`** | Base class for all errors; thrown with `throw`, caught with `try/catch` |
| **Custom exceptions** | Derive from `Exception` (or domain-specific base) |
| **Inner exception** | Chain exceptions with `new MyException("msg", innerEx)` to preserve causal context |
| **`catch (T ex) when ()`** | Filter expression — catch only when a condition is true |
| **`finally`** | Always-runs cleanup block (prefer `using` for `IDisposable`) |
| **Result pattern** | Return `Result<T>` instead of throwing for *expected* failures |

---

## 2. Implementation Examples

### Custom exceptions

```csharp
public class AccountNotFoundException : Exception
{
    public Guid AccountId { get; }

    public AccountNotFoundException(Guid accountId)
        : base($"Account {accountId} not found.")
    {
        AccountId = accountId;
    }

    // Chaining -- preserves the original exception as context
    public AccountNotFoundException(Guid accountId, Exception inner)
        : base($"Account {accountId} not found.", inner)
    {
        AccountId = accountId;
    }
}
```

### Catching specific exception types

```csharp
try
{
    await bankService.GetAccountAsync(id);
}
catch (AccountNotFoundException ex)
{
    // Matches exactly this type (and derived types)
    return NotFound(ex.Message);
}
catch (Exception ex) when (ex is not OperationCanceledException)
{
    // Catch-when: filter clause — don't swallow cancellation
    logger.LogError(ex, "Unexpected error");
    return StatusCode(500);
}
```

### Result pattern (alternative to exceptions for expected failures)

```csharp
public readonly record struct Result<T>
{
    public T? Value { get; }
    public string? Error { get; }
    public bool IsSuccess => Error is null;

    private Result(T value) => Value = value;
    private Result(string error) => Error = error;

    public static Result<T> Ok(T value) => new(value);
    public static Result<T> Fail(string error) => new(error);
}

// Usage
var result = TryParse("abc");
if (!result.IsSuccess)
    Console.WriteLine(result.Error);
```

---

## 3. When to use Exceptions vs Result

| Use exceptions when... | Use Result<T> when... |
| :--- | :--- |
| Unexpected / exceptional conditions | Expected failures (parse errors, not found) |
| The caller can't reasonably handle it | The caller *must* handle the failure path |
| Crossing a boundary (API, domain) | Internal domain logic |

---

## ⚠️ Pitfalls & Best Practices

> [!WARNING]
> Never catch `Exception` without re-throwing or logging. Swallowing exceptions hides bugs.

1. Don't use exceptions for flow control — they're slow and misleading.
2. Always include meaningful messages and relevant data in custom exceptions.
3. Use `finally` (or `using`) to release resources — it runs even if an exception is thrown.
4. Prefer specific exception types over generic `Exception` — it makes `catch` blocks precise.

---

## 🏃 Running the Examples

```bash
dotnet test tests/Basics.Tests --filter "FullyQualifiedName~ErrorHandling"
```

---

## 📚 Further Reading

- [Exception handling (C# docs)](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/exceptions/)
- [Creating custom exceptions](https://learn.microsoft.com/en-us/dotnet/standard/exceptions/how-to-create-user-defined-exceptions)

<details>
<summary>Coming from Go?</summary>

| Go | C# |
|---|---|
| `errors.New("msg")` | `new Exception("msg")` or custom exception |
| `fmt.Errorf("ctx: %w", err)` | `new MyException("ctx", innerException)` |
| `errors.Is(err, ErrNotFound)` | `catch (NotFoundException)` |
| `errors.As(err, &target)` | `catch (MyException ex)` -- pattern matching |
| `(T, error)` return | `Result<T>` pattern (or just throw for exceptional cases) |
| Sentinel errors | Custom exception types |
| `panic` / `recover` | `throw` / `try-catch` |

</details>

## Your Next Step
Once you can handle errors effectively, you can start defining the data structures that model your domain.
Explore **[Entities](../Entity/README.md)** to learn how `class`, `record`, and `struct` differ in C#.
