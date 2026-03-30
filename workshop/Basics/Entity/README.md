# Records, Classes & Structs in C#

C# provides four distinct type declarations -- `class`, `struct`, `record`, and `record struct` -- each with different semantics for equality, mutability, and memory allocation. Choosing the right one is a key design decision.

---

## 1. Core Concepts

| Type | Value/Ref | Equality | Mutability | Use when... |
| :--- | :--- | :--- | :--- | :--- |
| `class` | Reference | Reference (identity) | Mutable by default | Complex objects, shared state |
| `struct` | Value | Member-wise (by default) | Mutable unless `readonly` | Small, stack-friendly, no heap allocation |
| `record class` | Reference | **Value** (auto-generated) | Immutable by default | DTOs, immutable domain objects |
| `record struct` | Value | **Value** (auto-generated) | Mutable unless `readonly` | Immutable value types |

---

## 2. Implementation Examples

### `record` — immutable value-equal DTO

```csharp
public record Account(Guid Id, string Owner, decimal Balance);

var a1 = new Account(Guid.NewGuid(), "Alice", 100m);
var a2 = a1 with { Balance = 200m }; // non-destructive mutation

// Records have auto value equality
var copy = new Account(a1.Id, a1.Owner, a1.Balance);
Console.WriteLine(a1 == copy); // true!
```

### `class` — mutable reference type

```csharp
public class BankAccount
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Owner { get; init; }
    public decimal Balance { get; private set; }

    public void Deposit(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        Balance += amount;
    }
}
```

### `struct` — stack-allocated value type

```csharp
public readonly struct Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency) { Amount = amount; Currency = currency; }
    public Money Add(Money other) => new(Amount + other.Amount, Currency);
    public override string ToString() => $"{Amount} {Currency}";
}
```

---

## 3. JSON Serialization

C# uses `System.Text.Json` attributes to control serialization behavior:

```csharp
public record TransferRequest
{
    [JsonPropertyName("from_account_id")]
    public Guid FromAccountId { get; init; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }
}
```

---

## ⚠️ Pitfalls & Best Practices

1. Use `record` for DTOs and domain value objects — equality and `ToString()` come free.
2. Use `class` when you need reference identity or complex mutable state.
3. Use `struct` for small (<= 16 bytes) immutable data that's copied frequently.
4. `required` properties (C# 11+) force callers to initialize them — better than nullable constructor params.

---

## 🏃 Running the Examples

```bash
dotnet test tests/Basics.Tests --filter "FullyQualifiedName~Entity"
```

---

## 📚 Further Reading

- [Records (C# docs)](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record)
- [Choosing between class and struct](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/choosing-between-class-and-struct)
- [System.Text.Json attributes](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/required-properties)

---

## Your Next Step
Now that you're defining your own data structures, it's time to add behaviour to them using methods.
Explore **[Receivers & Extension Methods](../Receivers/README.md)** to learn how to define instance methods and extend existing types.
