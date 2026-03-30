namespace DotNetTraining.Basics.Parameters;

public static class ParameterExamples
{
    // ── Value parameters (default) ────────────────────────────────────────────

    /// <summary>Pass by value — caller's variable is NOT changed.</summary>
    public static int IncrementValue(int n) => n + 1;

    // ── ref / out / in ────────────────────────────────────────────────────────

    /// <summary>
    /// ref — pass by reference, caller's variable IS changed.
    /// </summary>
    public static void IncrementRef(ref int n) => n++;

    /// <summary>
    /// out — output parameter. Compiler enforces assignment before return.
    /// </summary>
    public static bool TryParsePositive(string input, out int result)
    {
        if (int.TryParse(input, out result) && result > 0)
        {
            return true;
        }

        result = 0;
        return false;
    }

    /// <summary>
    /// in — read-only reference. No copy made, but mutation is forbidden.
    /// Use for large structs you want to pass cheaply without modification.
    /// </summary>
    public static double DistanceBetween(in (double X, double Y) a, in (double X, double Y) b)
    {
        double dx = a.X - b.X;
        double dy = a.Y - b.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    // ── params — variadic arguments ───────────────────────────────────────────

    /// <summary>
    /// params allows calling with any number of arguments.
    /// </summary>
    public static int Sum(params int[] nums)
    {
        int total = 0;
        foreach (var n in nums)
        {
            total += n;
        }

        return total;
    }

    /// <summary>
    /// params with ReadOnlySpan (.NET 9+) — avoids heap allocation for small calls.
    /// </summary>
    public static double Average(params ReadOnlySpan<double> values)
    {
        if (values.IsEmpty)
        {
            return 0;
        }

        double sum = 0;
        foreach (var v in values)
        {
            sum += v;
        }

        return sum / values.Length;
    }

    // ── Optional parameters and named arguments ───────────────────────────────

    /// <summary>
    /// Optional parameters with defaults.
    /// CancellationToken = default is idiomatic for async methods.
    /// </summary>
    public static string FormatName(
        string first,
        string last,
        string separator = " ",
        bool upperCase = false)
    {
        var result = $"{first}{separator}{last}";
        return upperCase ? result.ToUpperInvariant() : result;
    }
}
