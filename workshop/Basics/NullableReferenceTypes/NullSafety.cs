namespace DotNetTraining.Basics.NullableReferenceTypes;

// ── Nullable Reference Types (NRT) ──────────────────────────────────────────
//
// Enabled by default in .NET 10 via <Nullable>enable</Nullable>.
// The compiler tracks null-state and warns when you dereference a possibly-null value.
// Key annotations: T? (nullable), T (non-null), ! (null-forgiving operator).

/// <summary>
/// Demonstrates nullable reference type annotations and null-safe patterns.
/// </summary>
public static class NullSafety
{
    /// <summary>
    /// Greet a user by name. Returns a default greeting if name is null.
    /// Demonstrates: null-coalescing operator ??.
    /// </summary>
    public static string Greet(string? name)
        => $"Hello, {name ?? "stranger"}!";

    /// <summary>
    /// Get the length of a string, or null if the string is null.
    /// Demonstrates: null-conditional operator ?. with nullable value type return.
    /// </summary>
    public static int? SafeLength(string? value)
        => value?.Length;

    /// <summary>
    /// Get the uppercase version of a string, or a fallback.
    /// Demonstrates: chaining ?. with ?? for safe transformation.
    /// </summary>
    public static string ToUpperOrDefault(string? value, string fallback = "N/A")
        => value?.ToUpperInvariant() ?? fallback;

    /// <summary>
    /// Assign a default value to a ref variable if it's null.
    /// Demonstrates: null-coalescing assignment ??=.
    /// </summary>
    public static string EnsureNotNull(ref string? value, string defaultValue)
    {
        value ??= defaultValue;
        return value;
    }
}

/// <summary>
/// A customer record demonstrating required non-null properties with optional nullable fields.
/// </summary>
public record Customer
{
    public required string Name { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }

    /// <summary>
    /// Returns the preferred contact method, falling through nullable fields.
    /// Demonstrates: chained ?? for multi-level fallback.
    /// </summary>
    public string PreferredContact => Email ?? Phone ?? "no contact info";
}

/// <summary>
/// Demonstrates null guard patterns and pattern matching with null.
/// </summary>
public static class NullGuards
{
    /// <summary>
    /// Validates that a required parameter is not null.
    /// Demonstrates: ArgumentNullException.ThrowIfNull (modern .NET guard).
    /// </summary>
    public static string ProcessName(string? name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return name.Trim().ToUpperInvariant();
    }

    /// <summary>
    /// Categorize a value using pattern matching, including null.
    /// Demonstrates: switch expression with null pattern and type patterns.
    /// </summary>
    public static string Describe(object? value) => value switch
    {
        null => "nothing",
        string s when s.Length == 0 => "empty string",
        string s => $"string: {s}",
        int n => $"number: {n}",
        _ => $"other: {value}"
    };

    /// <summary>
    /// Safely get a value from a dictionary, returning null for missing keys.
    /// Demonstrates: the is-not-null pattern and TryGetValue.
    /// </summary>
    public static string? SafeLookup(Dictionary<string, string> dict, string key)
        => dict.TryGetValue(key, out var value) ? value : null;
}
