using System.Diagnostics.CodeAnalysis;

namespace DotNetTraining.Basics.ModernSyntax;

// ── Primary Constructors (C# 12) ──────────────────────────────────────────────

/// <summary>
/// Primary constructor: parameters declared in the class header.
/// No separate constructor body needed for simple assignment.
/// Parameters are captured as fields by the compiler.
/// </summary>
public class Point(int x, int y)
{
    public int X => x;
    public int Y => y;

    public double DistanceTo(Point other) =>
        Math.Sqrt(Math.Pow(x - other.X, 2) + Math.Pow(y - other.Y, 2));
}

/// <summary>Primary constructors work on structs too.</summary>
public readonly struct Size(double width, double height)
{
    public double Width => width;
    public double Height => height;
    public double Area => width * height;
}

/// <summary>
/// Common DI pattern: primary constructor receives injected services.
/// More concise than a full constructor body with field assignments.
/// </summary>
public class OrderService(IEnumerable<string> catalogue)
{
    public bool Exists(string name) => catalogue.Contains(name, StringComparer.OrdinalIgnoreCase);
}

// ── Collection Expressions (C# 12) ───────────────────────────────────────────

public static class CollectionExpressions
{
    /// <summary>
    /// [e1, e2, ..] works for arrays, List&lt;T&gt;, Span&lt;T&gt;, ImmutableArray&lt;T&gt;, and more.
    /// The compiler picks the most efficient construction strategy.
    /// </summary>
    public static int[] GetPrimes() => [2, 3, 5, 7, 11];

    public static List<string> GetColours() => ["red", "green", "blue"];

    /// <summary>
    /// Spread operator `..` — inlines the elements of another collection.
    /// Equivalent to Concat but with no heap allocations for arrays.
    /// </summary>
    public static int[] Combine(int[] a, int[] b) => [.. a, .. b];

    /// <summary>Empty collection literal — works for any collection type.</summary>
    public static string[] Empty() => [];
}

// ── `with` Expressions (C# 9) ────────────────────────────────────────────────

public record Address(string Street, string City, string PostCode);
public record Customer(string Name, string Email, Address Address);

public static class WithExpressionExamples
{
    /// <summary>
    /// `with` creates a copy of a record with the specified properties changed.
    /// The original is never mutated — this is non-destructive mutation.
    /// </summary>
    public static Customer UpdateEmail(Customer customer, string newEmail) =>
        customer with { Email = newEmail };

    /// <summary>
    /// Nested `with` — update a nested record member.
    /// </summary>
    public static Customer MoveCity(Customer customer, string newCity) =>
        customer with { Address = customer.Address with { City = newCity } };
}

// ── Record Value Equality ─────────────────────────────────────────────────────

public record Money(decimal Amount, string Currency);

public static class RecordEqualityExamples
{
    /// <summary>
    /// Records compare by VALUE, not by reference.
    /// Two separate `new Money(10m, "USD")` instances are equal.
    /// </summary>
    public static bool AreEqual(Money a, Money b) => a == b;

    /// <summary>
    /// Positional records support deconstruction via compiler-generated Deconstruct().
    /// </summary>
    public static (decimal amount, string currency) Unpack(Money m)
    {
        var (amount, currency) = m;
        return (amount, currency);
    }
}

// ── Required and init-only properties ────────────────────────────────────────

/// <summary>
/// `required` (C# 11) — the compiler enforces that these properties are set
/// at the point of construction (object initialiser or custom constructor with
/// [SetsRequiredMembers]).
/// `init` accessor — settable only during initialisation, immutable afterwards.
/// </summary>
public class UserProfile
{
    public required string Username { get; init; }
    public required string Email { get; init; }
    public string? DisplayName { get; init; }   // optional
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Custom constructor that sets required properties.
/// [SetsRequiredMembers] tells the compiler all required members are initialised,
/// so callers don't also have to set them via object initialiser.
/// </summary>
public class ApiKey
{
    public required string Key { get; init; }
    public required string Service { get; init; }
    public DateTimeOffset IssuedAt { get; init; }

    [SetsRequiredMembers]
    public ApiKey(string key, string service)
    {
        Key = key;
        Service = service;
        IssuedAt = DateTimeOffset.UtcNow;
    }
}
