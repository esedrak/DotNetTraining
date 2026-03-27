namespace DotNetTraining.Basics.Pointers;

// ── Value Types (struct) ────────────────────────────────────────────────────

/// <summary>Demonstrates that structs are copied on assignment.</summary>
public struct Point
{
    public int X;
    public int Y;
}

/// <summary>Demonstrates that classes share a reference on assignment.</summary>
public class PointClass
{
    public int X;
    public int Y;
}

// ── ref / out / in keywords ─────────────────────────────────────────────────

public static class RefOutExamples
{
    /// <summary>
    /// Increment takes n by value — caller's variable is NOT changed.
    /// Equivalent to Go: func IncrementValue(n int) int
    /// </summary>
    public static int IncrementValue(int n) => n + 1;

    /// <summary>
    /// IncrementRef takes n by reference — caller's variable IS changed.
    /// Equivalent to Go: func IncrementPointer(n *int)
    /// </summary>
    public static void IncrementRef(ref int n) => n++;

    /// <summary>
    /// TryDouble uses an out parameter for a required output.
    /// The compiler enforces that result is assigned before returning.
    /// </summary>
    public static bool TryDouble(string input, out int result)
    {
        if (int.TryParse(input, out int parsed) && parsed >= 0)
        {
            result = parsed * 2;
            return true;
        }
        result = 0;
        return false;
    }

    /// <summary>
    /// SumLarge takes a large struct by read-only reference to avoid copying.
    /// The `in` modifier prevents mutation while avoiding the copy overhead.
    /// </summary>
    public static long SumLarge(in Point p) => p.X + p.Y;
}

// ── Nullable value types ─────────────────────────────────────────────────────

public static class NullableExamples
{
    /// <summary>
    /// FindFirst returns null if the predicate matches nothing.
    /// int? is shorthand for Nullable&lt;int&gt; — equivalent to a nil-able value.
    /// </summary>
    public static int? FindFirst(IEnumerable<int> source, Func<int, bool> predicate)
    {
        foreach (var item in source)
        {
            if (predicate(item))
            {
                return item;
            }
        }
        return null; // "no value found"
    }
}

// ── Counter: stateful struct with ref semantics via class ────────────────────

/// <summary>
/// Counter uses a class so it can be shared by reference.
/// Equivalent to Go's pointer receiver pattern.
/// </summary>
public class Counter
{
    private int _count;

    public void Increment() => _count++;
    public int Value => _count;
}

// ── Span&lt;T&gt; ────────────────────────────────────────────────────────────────

public static class SpanExamples
{
    /// <summary>
    /// Sum uses Span&lt;T&gt; to read a contiguous block of memory without allocation.
    /// Works with arrays, stack-allocated buffers, and Memory&lt;T&gt;.
    /// </summary>
    public static int Sum(ReadOnlySpan<int> values)
    {
        int total = 0;
        foreach (var v in values)
        {
            total += v;
        }

        return total;
    }
}
