# Parameters in C#

C# provides a rich parameter system including optional defaults, named arguments, `params` variadic parameters, and `ref`/`out`/`in` modifiers. These features give you fine-grained control over how data flows into and out of methods.

---

## 1. Core Concepts

| Concept | Description |
| :--- | :--- |
| **Value parameters** | Default — a copy is passed to the method |
| **`ref` parameter** | Pass by reference — callee can mutate the caller's variable |
| **`out` parameter** | Output-only reference — callee *must* assign before returning |
| **`in` parameter** | Read-only reference — no copy, no mutation |
| **`params T[]`** | Variadic parameter — accepts a variable number of arguments |
| **Optional parameters** | Default values: `void F(int x = 0)` |
| **Named arguments** | `F(y: 2, x: 1)` — call in any order |

---

## 2. Examples

### Optional parameters (default values)

```csharp
// All three parameters have defaults — callers can omit them
public Task<Account[]> ListAccountsAsync(
    int page = 1,
    int pageSize = 20,
    CancellationToken cancellationToken = default)
{ ... }

// All of these are valid:
await ListAccountsAsync();
await ListAccountsAsync(page: 2);
await ListAccountsAsync(2, 50, token);
```

### `params` -- variadic arguments

```csharp
public static int Sum(params int[] nums) => nums.Sum();
public static int Sum(params ReadOnlySpan<int> nums) // .NET 9+ — avoids heap allocation
{
    int total = 0;
    foreach (var n in nums) total += n;
    return total;
}

Sum(1, 2, 3);          // 6
Sum([.. new[]{1,2,3}]); // same via spread
```

### Named arguments

```csharp
// Improve readability with boolean/ambiguous parameters
CreateAccount(owner: "Alice", initialBalance: 100m, isVerified: true);
```

---

## ⚠️ Pitfalls & Best Practices

1. Optional parameters bake the default into the caller at compile time — changing a default is a breaking change for assemblies that aren't recompiled.
2. Prefer `out` over returning a tuple when the "primary" return is success/failure.
3. Use named arguments for boolean parameters — `Process(force: true)` is clearer than `Process(true)`.
4. Use `params ReadOnlySpan<T>` (.NET 9+) instead of `params T[]` for performance-sensitive hot paths.

---

## 🏃 Running the Examples

```bash
dotnet test tests/Basics.Tests --filter "FullyQualifiedName~Parameters"
```

---

## 📚 Further Reading

- [Method parameters (C# docs)](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/method-parameters)
- [Named and optional arguments](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/named-and-optional-arguments)

---

<details>
<summary>Coming from Go?</summary>

| Go | C# |
|---|---|
| `func F(n int)` | `void F(int n)` |
| `func F(p *int)` | `void F(ref int p)` |
| `func F() (int, error)` | `(int result, string error)` tuple, or `out` param |
| `func F(args ...int)` | `void F(params int[] args)` |
| N/A | `void F(int x = 0)` -- optional with default |
| N/A | `F(y: 2, x: 1)` -- named arguments |

</details>

## Your Next Step
With a solid grasp of how data flows through methods, you're ready to start handling the inevitable errors that occur at runtime.
Explore **[Error Handling](../ErrorHandling/README.md)** to learn C#'s idiomatic approach to exceptions and the `Result<T>` pattern.
