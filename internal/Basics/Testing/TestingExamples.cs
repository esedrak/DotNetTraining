namespace DotNetTraining.Basics.Testing;

// ── Types used by xUnit pattern examples ─────────────────────────────────────

/// <summary>
/// Simple calculator — the "system under test" in xUnit pattern examples
/// (see tests/Basics.Tests/TestingTests.cs).
/// Go equivalent: any package that has exported functions to test.
/// </summary>
public static class Calculator
{
    public static int Add(int a, int b) => a + b;
    public static int Subtract(int a, int b) => a - b;
    public static double Divide(double numerator, double denominator)
    {
        if (denominator == 0)
            throw new DivideByZeroException("Denominator cannot be zero.");
        return numerator / denominator;
    }
}

/// <summary>
/// A disposable resource — demonstrates <c>IClassFixture&lt;T&gt;</c> lifecycle.
/// </summary>
public sealed class DatabaseFixture : IDisposable
{
    public string ConnectionString { get; } = "Server=localhost;Database=test";
    public bool IsConnected { get; private set; }

    public DatabaseFixture()
    {
        // Simulate expensive one-time setup (e.g., spin up a test container)
        IsConnected = true;
    }

    public void Dispose()
    {
        IsConnected = false;
    }
}
