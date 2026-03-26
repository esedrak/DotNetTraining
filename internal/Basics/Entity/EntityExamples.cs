using System.Text.Json.Serialization;

namespace DotNetTraining.Basics.Entity;

// ── record class — immutable, value-equal DTO ─────────────────────────────────

/// <summary>
/// Positional record: compiler generates constructor, properties, Equals, GetHashCode, ToString.
/// Equivalent to a Go struct with manual equality methods.
/// </summary>
public record Account(Guid Id, string Owner, decimal Balance)
{
    /// <summary>Non-destructive mutation: `with` returns a new record with changed fields.</summary>
    public Account Deposit(decimal amount) => this with { Balance = Balance + amount };
    public Account Withdraw(decimal amount) => this with { Balance = Balance - amount };
}

// ── record with JSON attributes ───────────────────────────────────────────────

/// <summary>
/// Records work seamlessly with System.Text.Json.
/// Attributes replace Go's struct tags: json:"field_name" → [JsonPropertyName("field_name")]
/// </summary>
public record TransferRequest
{
    [JsonPropertyName("from_account_id")]
    public required Guid FromAccountId { get; init; }

    [JsonPropertyName("to_account_id")]
    public required Guid ToAccountId { get; init; }

    [JsonPropertyName("amount")]
    public required decimal Amount { get; init; }
}

// ── class — mutable reference type with encapsulation ────────────────────────

/// <summary>
/// Class-based domain entity — use when you need controlled mutation.
/// `required` (C# 11) enforces initialization in object initializers.
/// `init` makes the property settable only during construction.
/// </summary>
public class BankAccount
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Owner { get; init; }
    public decimal Balance { get; private set; }

    public void Deposit(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount, nameof(amount));
        Balance += amount;
    }

    public void Withdraw(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount, nameof(amount));
        if (amount > Balance)
            throw new InvalidOperationException("Insufficient funds.");
        Balance -= amount;
    }
}

// ── struct — stack-allocated value type ──────────────────────────────────────

/// <summary>
/// readonly struct: immutable, stack-allocated, copied by value.
/// Great for small, frequently-copied value objects like Money, Point, Color.
/// </summary>
public readonly struct Money : IEquatable<Money>
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount, nameof(amount));
        Amount = amount;
        Currency = currency;
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add {Currency} and {other.Currency}.");
        return new Money(Amount + other.Amount, Currency);
    }

    public bool Equals(Money other) => Amount == other.Amount && Currency == other.Currency;
    public override bool Equals(object? obj) => obj is Money other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Amount, Currency);
    public override string ToString() => $"{Amount:F2} {Currency}";

    public static bool operator ==(Money left, Money right) => left.Equals(right);
    public static bool operator !=(Money left, Money right) => !left.Equals(right);
}

// ── record struct — value type with auto equality ────────────────────────────

/// <summary>
/// record struct (C# 10): value type + auto-generated Equals/GetHashCode/ToString.
/// Best of both worlds for small immutable value types.
/// </summary>
public readonly record struct Coordinate(double Latitude, double Longitude)
{
    public double DistanceTo(Coordinate other)
    {
        var dlat = Math.Abs(Latitude - other.Latitude);
        var dlon = Math.Abs(Longitude - other.Longitude);
        return Math.Sqrt(dlat * dlat + dlon * dlon);
    }
}
