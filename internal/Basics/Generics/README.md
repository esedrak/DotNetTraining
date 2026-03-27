# Generics in C#

C# generics let you write type-safe, reusable code that works across multiple types without sacrificing performance or type information. They are **reified** at runtime -- the CLR retains full type information, enabling reflection, richer constraints, and covariance/contravariance on interfaces.

---

## 1. Core Concepts

| Concept | Description |
| :--- | :--- |
| **`T`** | Type parameter — placeholder for a concrete type |
| **`where T : constraint`** | Restrict what types are allowed |
| **`default(T)`** | Zero value for any type (0, false, null) |
| **Covariance `out T`** | Interfaces can be covariant for read-only producers |
| **Contravariance `in T`** | Interfaces can be contravariant for write-only consumers |

---

## 2. Common Constraints

| Constraint | Meaning |
| :--- | :--- |
| `where T : class` | T must be a reference type |
| `where T : struct` | T must be a value type |
| `where T : new()` | T must have a parameterless constructor |
| `where T : IComparable<T>` | T supports ordering |
| `where T : IEquatable<T>` | T supports equality |
| `where T : notnull` | T cannot be null |
| `where T : BaseClass` | T must derive from BaseClass |

---

## 3. Examples

### Generic method

```csharp
public static T Min<T>(T a, T b) where T : IComparable<T>
    => a.CompareTo(b) <= 0 ? a : b;

Min(3, 5);       // int
Min("a", "z");   // string
```

### Generic class — typed stack

```csharp
public class Stack<T>
{
    private readonly List<T> _items = [];

    public void Push(T item) => _items.Add(item);
    public T Pop() { var t = _items[^1]; _items.RemoveAt(_items.Count - 1); return t; }
    public bool IsEmpty => _items.Count == 0;
}
```

### Default value

```csharp
T GetDefault<T>() => default!;  // Returns 0, false, or null depending on T
```

---

## Pitfalls & Best Practices

1. Prefer interface constraints over class constraints — more flexible.
2. Use `where T : notnull` with nullable-enabled code to prevent accidental nulls.
3. Don't over-generalize — if you only use one or two concrete types, a generic might add complexity without benefit.

---

## Running the Examples

```bash
dotnet test tests/Basics.Tests --filter "FullyQualifiedName~Generics"
```

---

## Further Reading

- [Generic classes and methods](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/generics)
- [Constraints on type parameters](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/constraints-on-type-parameters)

---

<details>
<summary>Coming from Go?</summary>

| Go | C# |
| :--- | :--- |
| `func F[T any](x T)` | `void F<T>(T x)` |
| `[T comparable]` | `where T : IEquatable<T>` |
| `[T any]` | `<T>` (unconstrained) |
| `var zero T` | `default(T)` or `default` |
| No runtime type info | `typeof(T)`, reflection available at runtime |

</details>

## Your Next Step
Finally, you'll want to learn how to control which parts of your code are compiled based on the environment or specific build needs.
Explore **[Conditional Compilation](../ConditionalCompilation/README.md)** to master `#if DEBUG`, `DefineConstants`, and `RuntimeInformation`.
