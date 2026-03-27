# 🔄 Type Conversion & Pattern Matching in C#

C# has two mechanisms for working with types at runtime: **type conversion** (between compatible concrete types) and **pattern matching** (testing and extracting values from interfaces or `object`). These replace Go's type assertions and type switches.

---

## 1. Core Concepts

| Concept | Description |
| :--- | :--- |
| **Explicit cast `(T)x`** | Convert between compatible types; throws `InvalidCastException` on failure |
| **`as` operator** | Safe cast — returns `null` on failure instead of throwing |
| **`is` pattern** | Test type and bind in one expression: `if (x is Dog dog)` |
| **`switch` expression** | Full type-switch with patterns (replaces Go's `switch v := i.(type)`) |
| **`checked`** | Throw `OverflowException` on numeric overflow |

---

## 2. Go → C# Mapping

| Go | C# |
| :--- | :--- |
| `x.(T)` (unsafe assertion) | `(T)x` — throws on failure |
| `v, ok := x.(T)` (safe) | `x as T` + null check, or `x is T v` |
| `switch v := i.(type)` | `switch (x) { case T t: ... }` or switch expression |
| `interface{}` / `any` | `object` |
| Numeric cast `int(f)` | `(int)f` |

---

## 3. Implementation Examples

### Safe pattern matching (preferred)

```csharp
// Test + bind in one step
if (animal is Dog dog)
    Console.WriteLine(dog.Breed);

// Switch expression — exhaustive type matching
string result = shape switch
{
    Circle c  => $"circle r={c.Radius}",
    Rectangle r => $"rect {r.W}x{r.H}",
    _         => "unknown"
};
```

### `as` operator (returns null on failure)

```csharp
// Go equivalent: d, ok := animal.(Dog)
var dog = animal as Dog;
if (dog != null)
    Console.WriteLine(dog.Breed);
```

### Numeric conversions

```csharp
int i = 42;
double d = (double)i;   // widening — safe, explicit
int back = (int)3.99;   // narrowing — truncates to 3

// checked throws OverflowException instead of wrapping
byte safe = checked((byte)256); // throws!
```

---

## 4. Common Patterns

- Prefer **`is` + pattern variable** over `as` + null check — it's more expressive
- Use **switch expressions** for exhaustive type dispatching
- Use **`checked`** when overflow is a correctness concern (financial calculations, etc.)

---

## ⚠️ Pitfalls & Best Practices

> [!WARNING]
> A bare `(T)x` cast throws `InvalidCastException` at runtime if the types are incompatible. Prefer `is` patterns.

1. Don't use `as` on value types — the compiler rejects it (value types can't be `null`).
2. Numeric casts between value types never throw by default — they silently overflow. Use `checked`.
3. Prefer `switch` expressions over chains of `if (x is T)` for multiple type cases.

---

## 🏃 Running the Examples

```bash
dotnet test tests/Basics.Tests --filter "FullyQualifiedName~Casting"
```

---

## 📚 Further Reading

- [Pattern matching (C# docs)](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/pattern-matching)
- [Type-testing operators](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/type-testing-and-cast)
- [Checked and unchecked](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/checked-and-unchecked)

## Your Next Step
Now that you understand how to work with interfaces and dynamic types, you can explore how C# uses composition and embedded resources.
Explore **[Embedding](../Embedding/README.md)** to see how to compose types and bundle files into your assemblies.
