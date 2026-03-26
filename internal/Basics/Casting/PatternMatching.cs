namespace DotNetTraining.Basics.Casting;

// ── Type checking and casting ────────────────────────────────────────────────

public interface IAnimal
{
    string Speak();
}

public class Dog : IAnimal
{
    public string Breed { get; init; } = "";
    public string Speak() => "Woof!";
}

public class Cat : IAnimal
{
    public string Speak() => "Meow!";
}

public static class TypeCheckingExamples
{
    /// <summary>
    /// Safe cast using 'as' — returns null on failure (no exception).
    /// Equivalent to Go's comma-ok type assertion: d, ok := i.(Dog)
    /// </summary>
    public static string? GetBreed(IAnimal animal)
    {
        var dog = animal as Dog;  // null if not a Dog
        return dog?.Breed;
    }

    /// <summary>
    /// Pattern matching with 'is' — the modern C# approach.
    /// More concise than 'as' + null check.
    /// </summary>
    public static string Describe(IAnimal animal)
    {
        if (animal is Dog dog)
            return $"Dog, breed: {dog.Breed}";
        if (animal is Cat)
            return "Cat";
        return "Unknown animal";
    }

    /// <summary>
    /// Switch expression with type patterns.
    /// Equivalent to Go's type switch: switch v := i.(type)
    /// </summary>
    public static string WhatIsIt(object value) => value switch
    {
        string s => $"string: {s}",
        int i    => $"int: {i}",
        bool b   => $"bool: {b}",
        Dog dog  => $"Dog breed: {dog.Breed}",
        null     => "null",
        _        => $"unknown: {value.GetType().Name}"
    };
}

// ── Numeric conversions ──────────────────────────────────────────────────────

public static class ConversionExamples
{
    /// <summary>
    /// C# never performs implicit numeric conversion (same as Go).
    /// You must cast explicitly. Narrowing conversions can lose data.
    /// </summary>
    public static void NumericConversions()
    {
        int i = 42;
        double d = (double)i;   // widening — safe
        int back = (int)d;      // narrowing — truncates

        double pi = 3.14159;
        int truncated = (int)pi; // 3 — fractional part lost

        long big = 257L;
        byte small = (byte)big; // 1 — overflow wraps (in unchecked context)
        _ = back; _ = small;
    }

    /// <summary>
    /// Use checked{} to throw OverflowException on overflow instead of silently wrapping.
    /// </summary>
    public static byte SafeNarrow(long value)
    {
        return checked((byte)value); // throws OverflowException if > 255
    }

    /// <summary>
    /// String ↔ byte/char conversions (equivalent to Go's []byte / []rune conversions).
    /// </summary>
    public static void StringConversions()
    {
        string s = "hello";
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(s);
        char[] chars = s.ToCharArray();
        string back = new string(chars);
        _ = bytes; _ = back;
    }
}
