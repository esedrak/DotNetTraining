# 📬 Methods & Extension Methods in C#

Go has **value receivers** and **pointer receivers**. C# uses **instance methods on classes** (always reference semantics) and **instance methods on structs** (value semantics). Additionally, C# has **extension methods** — a way to add methods to existing types without modifying them.

---

## 1. Core Concepts

| Concept | Description |
| :--- | :--- |
| **Instance method** | Defined inside the type; has implicit access to `this` |
| **Static method** | No `this`; utility/factory functions |
| **Extension method** | Defined in a static class; `this T param` makes it callable on T |
| **C# 14 `extension` block** | New syntax — cleaner extension method grouping |

---

## 2. Go → C# Mapping

| Go | C# |
| :--- | :--- |
| `func (v T) Method()` — value receiver | Method on a `struct` (copies the struct) |
| `func (p *T) Method()` — pointer receiver | Method on a `class` (reference, no copy) |
| N/A | Extension methods on any type including sealed/third-party |
| N/A | C# 14 `extension` blocks |
| Static functions | `static` methods |

---

## 3. Examples

### Value vs reference semantics

```csharp
// struct method — operates on a copy (like Go value receiver)
public struct Temperature
{
    public double Celsius { get; }
    public Temperature(double c) => Celsius = c;
    public double ToFahrenheit() => Celsius * 9/5 + 32;  // read-only, no mutation
}

// class method — operates on the same instance (like Go pointer receiver)
public class BankAccount
{
    public decimal Balance { get; private set; }
    public void Deposit(decimal amount) => Balance += amount;  // mutates this
}
```

### Extension methods

```csharp
// Defined in a static class — the `this` parameter targets the type
public static class StringExtensions
{
    public static bool IsNullOrEmpty(this string? s) => string.IsNullOrEmpty(s);
    public static string Truncate(this string s, int maxLength)
        => s.Length <= maxLength ? s : s[..maxLength] + "...";
}

// Called like an instance method
"hello world".Truncate(5);   // "hello..."
((string?)null).IsNullOrEmpty(); // true
```

### C# 14 `extension` blocks (new!)

```csharp
// Cleaner syntax for grouping extensions — preview feature in C# 14
extension(string s)
{
    public bool IsPalindrome()
    {
        var reversed = new string(s.Reverse().ToArray());
        return s.Equals(reversed, StringComparison.OrdinalIgnoreCase);
    }
}
```

---

## ⚠️ Pitfalls & Best Practices

1. Extension methods don't break encapsulation — they can only access public members.
2. Avoid extension methods on `object` — they pollute IntelliSense for every type.
3. Put extension methods in a namespace that matches the type being extended.
4. Prefer instance methods over extension methods when you control the type.

---

## 🏃 Running the Examples

```bash
dotnet test tests/Basics.Tests --filter "FullyQualifiedName~Receivers"
```

---

## 📚 Further Reading

- [Extension methods (C# docs)](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods)
- [C# 14 extension members (preview)](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
