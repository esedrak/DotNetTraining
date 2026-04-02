namespace DotNetTraining.Basics.AdvancedPatternMatching;

// ── Relational patterns ───────────────────────────────────────────────────────

public static class RangePatterns
{
    /// <summary>
    /// Relational patterns: &lt;, &gt;, &lt;=, &gt;= directly in switch arms.
    /// No intermediate variable needed.
    /// </summary>
    public static string Classify(int score) => score switch
    {
        < 0 => "invalid",
        < 60 => "fail",
        < 80 => "pass",
        < 90 => "merit",
        <= 100 => "distinction",
        _ => "invalid"
    };

    public static string Temperature(double celsius) => celsius switch
    {
        < 0 => "freezing",
        >= 0 and < 15 => "cold",
        >= 15 and < 25 => "mild",
        >= 25 => "hot",
        _ => "unknown"
    };
}

// ── Logical patterns (and / or / not) ────────────────────────────────────────

public static class LogicalPatterns
{
    /// <summary>
    /// `or` — match any of multiple patterns.
    /// </summary>
    public static bool IsWeekend(DayOfWeek day) =>
        day is DayOfWeek.Saturday or DayOfWeek.Sunday;

    /// <summary>
    /// `and` — both conditions must match.
    /// </summary>
    public static bool IsWorkingHour(int hour) =>
        hour is >= 9 and <= 17;

    /// <summary>
    /// `not` — negate a pattern.
    /// Combined with type and property patterns.
    /// </summary>
    public static string Describe(object? obj) => obj switch
    {
        null => "null",
        not string => "not a string",
        string { Length: 0 } => "empty string",
        string s => $"string: \"{s}\""
    };
}

// ── Property patterns (including extended C# 10 nested property patterns) ────

public record Address(string Street, string City, string Country);
public record Person(string Name, int Age, Address Address);

public static class PropertyPatterns
{
    /// <summary>
    /// Extended property pattern (C# 10): access nested members with dot notation
    /// instead of nesting braces — { Address.City: "London" }.
    /// </summary>
    public static bool LivesInLondon(Person p) =>
        p is { Address.City: "London" };

    public static string Describe(Person p) => p switch
    {
        { Age: < 18 } => "minor",
        { Age: >= 65 } => "senior",
        { Address.Country: "US" } => "US adult",
        _ => "adult"
    };
}

// ── Positional patterns ───────────────────────────────────────────────────────

/// <summary>
/// A type with a Deconstruct method can be matched positionally.
/// Records generate Deconstruct automatically.
/// </summary>
public record Point(int X, int Y);

public static class PositionalPatterns
{
    public static string Quadrant(Point p) => p switch
    {
        (0, 0) => "origin",
        ( > 0, > 0) => "Q1",
        ( < 0, > 0) => "Q2",
        ( < 0, < 0) => "Q3",
        ( > 0, < 0) => "Q4",
        _ => "axis"
    };
}

// ── List patterns (C# 11) ─────────────────────────────────────────────────────

public static class ListPatterns
{
    /// <summary>
    /// List patterns match the structure of an array or list.
    /// `..` is the slice pattern — matches zero or more elements.
    /// </summary>
    public static string DescribeList(int[] values) => values switch
    {
        [] => "empty",
        [var single] => $"one element: {single}",
        [var first, var second] => $"two elements: {first}, {second}",
        [var first, .., var last] => $"starts {first}, ends {last}"
    };

    /// <summary>Matches any array that starts with the value 1.</summary>
    public static bool StartsWithOne(int[] values) =>
        values is [1, ..];

    /// <summary>Matches arrays with exactly three elements (any values).</summary>
    public static bool HasExactlyThree(int[] values) =>
        values is [_, _, _];
}

// ── Switch expression with property pattern guards ────────────────────────────

public record Order(decimal Amount, string Status, bool IsPriority);

public static class GuardPatterns
{
    /// <summary>
    /// Property patterns combined with relational patterns inside a single switch.
    /// Order of arms matters — more specific arms must come first.
    /// </summary>
    public static decimal CalculateDiscount(Order order) => order switch
    {
        { Status: "cancelled" } => 0m,
        { Amount: > 1000, IsPriority: true } => order.Amount * 0.15m,
        { Amount: > 1000 } => order.Amount * 0.10m,
        { IsPriority: true } => order.Amount * 0.05m,
        _ => 0m
    };
}
