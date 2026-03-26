# 🧬 Generics in C#

C# generics are **reified** at runtime — the CLR retains full type information, unlike Go's compile-time monomorphization. The syntax is similar (`<T>`), but C# offers richer constraints and runtime reflection.

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

## 2. Go → C# Mapping

| Go | C# |
| :--- | :--- |
| `func F[T any](x T)` | `void F<T>(T x)` |
| `[T comparable]` | `where T : IEquatable<T>` |
| `[T any]` | `<T>` (unconstrained) |
| `var zero T` | `default(T)` or `default` |
| No runtime type info | `typeof(T)`, reflection available at runtime |

---

## 3. Common Constraints

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

## 4. Examples

### Generic method

```csharp
// Go: func Min[T constraints.Ordered](a, b T) T
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
// Go: var zero T
T GetDefault<T>() => default!;  // 0, false, null depending on T
```

---

## ⚠️ Pitfalls & Best Practices

1. Prefer interface constraints over class constraints — more flexible.
2. Use `where T : notnull` with nullable-enabled code to prevent accidental nulls.
3. Don't over-generalize — if you only use one or two concrete types, a generic might add complexity without benefit.

---

## 🏃 Running the Examples

```bash
dotnet test tests/Basics.Tests --filter "FullyQualifiedName~Generics"
```

---

## 📚 Further Reading

- [Generic classes and methods](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/generics)
- [Constraints on type parameters](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/constraints-on-type-parameters)
