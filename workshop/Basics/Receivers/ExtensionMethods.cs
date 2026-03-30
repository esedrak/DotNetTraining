namespace DotNetTraining.Basics.Receivers;

// ── Value type (struct) methods — copy semantics ──────────────────────────────

/// <summary>
/// Struct methods operate on a copy of the struct
/// Mutation requires returning a new value.
/// </summary>
public readonly struct Temperature
{
    public double Celsius { get; }
    public Temperature(double celsius) => Celsius = celsius;

    // Read-only method
    public double ToFahrenheit() => Celsius * 9.0 / 5.0 + 32.0;
    public double ToKelvin() => Celsius + 273.15;
    public Temperature Add(double degrees) => new(Celsius + degrees);

    public override string ToString() => $"{Celsius:F1}°C";
}

// ── Class methods — reference semantics ──────────────────────────────────────

/// <summary>
/// Class methods operate on the same instance
/// Mutations are visible to all references.
/// </summary>
public class Counter
{
    public int Value { get; private set; }

    // Mutating method
    public void Increment() => Value++;
    public void Reset() => Value = 0;

    // Read-only method — still on a reference; no copy made
    public bool IsZero() => Value == 0;
}

// ── Extension methods ─────────────────────────────────────────────────────────

/// <summary>
/// Extension methods on string — adds utility methods without inheriting from string
/// (which is sealed).
/// </summary>
public static class StringExtensions
{
    public static bool IsNullOrEmpty(this string? s) => string.IsNullOrEmpty(s);
    public static bool IsNullOrWhiteSpace(this string? s) => string.IsNullOrWhiteSpace(s);

    /// <summary>Truncate to maxLength, appending "…" if trimmed.</summary>
    public static string Truncate(this string s, int maxLength)
    {
        ArgumentNullException.ThrowIfNull(s);
        return s.Length <= maxLength ? s : string.Concat(s.AsSpan(0, maxLength), "...");
    }

    /// <summary>Convert "hello world" → "Hello World"</summary>
    public static string ToTitleCase(this string s)
        => System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
}

/// <summary>Extension methods on IEnumerable — functional helpers.</summary>
public static class EnumerableExtensions
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) where T : class
        => source.Where(x => x is not null)!;

    public static IEnumerable<(int Index, T Item)> WithIndex<T>(this IEnumerable<T> source)
        => source.Select((item, idx) => (idx, item));
    // Note: Chunk<T>(size) is built into .NET 6+ as System.Linq.Enumerable.Chunk
}

// ── Static utility methods (not extension) ────────────────────────────────────

public static class MathUtils
{
    public static int Clamp(int value, int min, int max) => Math.Clamp(value, min, max);
    public static bool IsBetween(double value, double min, double max) => value >= min && value <= max;
}
