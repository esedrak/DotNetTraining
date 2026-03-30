namespace DotNetTraining.Basics.Generics;

// ── Generic methods ───────────────────────────────────────────────────────────

public static class GenericMethods
{
    /// <summary>
    /// Min works for any type that supports ordering.
    /// </summary>
    public static T Min<T>(T a, T b) where T : IComparable<T>
        => a.CompareTo(b) <= 0 ? a : b;

    /// <summary>
    /// Returns the zero/default value for any type.
    /// </summary>
    public static T GetDefault<T>() => default!;

    /// <summary>
    /// Swap two values — works with any type.
    /// Demonstrates ref parameters with generics.
    /// </summary>
    public static void Swap<T>(ref T a, ref T b) => (a, b) = (b, a);

    /// <summary>
    /// Filter a sequence — equivalent to a generic functional predicate.
    /// </summary>
    public static IEnumerable<T> Where<T>(IEnumerable<T> source, Func<T, bool> predicate)
    {
        foreach (var item in source)
        {
            if (predicate(item))
            {
                yield return item;
            }
        }
    }
}

// ── Generic class — typed stack ───────────────────────────────────────────────
// Named GenericStack to avoid ambiguity with System.Collections.Generic.Stack<T>

public class GenericStack<T>
{
    private readonly List<T> _items = [];

    public void Push(T item) => _items.Add(item);

    public T Pop()
    {
        if (_items.Count == 0)
        {
            throw new InvalidOperationException("Stack is empty.");
        }

        var top = _items[^1];
        _items.RemoveAt(_items.Count - 1);
        return top;
    }

    public T Peek() => _items.Count > 0
        ? _items[^1]
        : throw new InvalidOperationException("Stack is empty.");

    public bool IsEmpty => _items.Count == 0;
    public int Count => _items.Count;
}

// ── Generic constraints ───────────────────────────────────────────────────────

/// <summary>
/// Repository contract using generic constraints.
/// T must be a reference type (class) and not null.
/// </summary>
public interface IRepository<T> where T : class
{
    T? FindById(int id);
    void Add(T entity);
}

/// <summary>
/// A generic result type using struct constraint to prevent null wrapping.
/// </summary>
public readonly struct Optional<T> where T : notnull
{
    private readonly T? _value;
    public bool HasValue { get; }

    private Optional(T value) { _value = value; HasValue = true; }

    public static Optional<T> Some(T value) => new(value);
    public static Optional<T> None() => default;

    public T Value => HasValue ? _value! : throw new InvalidOperationException("No value.");
    public T GetValueOrDefault(T fallback) => HasValue ? _value! : fallback;
    public override string ToString() => HasValue ? $"Some({_value})" : "None";
}
