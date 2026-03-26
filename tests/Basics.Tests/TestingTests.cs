using DotNetTraining.Basics.Testing;
using FluentAssertions;

namespace Basics.Tests;

/// <summary>
/// Demonstrates xUnit patterns — C# equivalent of Go's table-driven tests.
///
/// | Go                         | xUnit                        |
/// |----------------------------|------------------------------|
/// | func TestXxx(t *testing.T) | [Fact] method                |
/// | t.Run("name", ...)         | [Theory] + [InlineData]      |
/// | testing.T helper           | ITestOutputHelper            |
/// | TestMain                   | IClassFixture&lt;T&gt;            |
/// | t.Parallel()               | automatic in xUnit           |
/// </summary>
public class TestingTests
{
    // ── [Fact] — simplest test, no parameters ─────────────────────────────────

    [Fact]
    public void Add_TwoAndThree_ReturnsFive()
    {
        Calculator.Add(2, 3).Should().Be(5);
    }

    // ── [Theory] + [InlineData] — table-driven, Go equivalent: t.Run ─────────

    [Theory]
    [InlineData(1, 1, 2)]
    [InlineData(0, 0, 0)]
    [InlineData(-1, 1, 0)]
    [InlineData(100, -50, 50)]
    public void Add_TableDriven(int a, int b, int expected)
    {
        Calculator.Add(a, b).Should().Be(expected);
    }

    [Theory]
    [InlineData(10.0, 2.0, 5.0)]
    [InlineData(9.0, 3.0, 3.0)]
    [InlineData(1.0, 4.0, 0.25)]
    public void Divide_ReturnsQuotient(double a, double b, double expected)
    {
        Calculator.Divide(a, b).Should().BeApproximately(expected, 0.0001);
    }

    // ── Exception testing ─────────────────────────────────────────────────────

    [Fact]
    public void Divide_ByZero_ThrowsDivideByZeroException()
    {
        var act = () => Calculator.Divide(1.0, 0.0);
        act.Should().Throw<DivideByZeroException>();
    }

    // ── [MemberData] — external data source (more complex tables) ────────────

    public static IEnumerable<object[]> SubtractCases =>
    [
        [10, 3, 7],
        [5, 5, 0],
        [-1, -1, 0],
    ];

    [Theory]
    [MemberData(nameof(SubtractCases))]
    public void Subtract_TableDriven(int a, int b, int expected)
    {
        Calculator.Subtract(a, b).Should().Be(expected);
    }
}

/// <summary>
/// Demonstrates IClassFixture — shared expensive setup across all tests in a class.
/// Go equivalent: TestMain with shared state, or sync.Once.
/// </summary>
public class DatabaseFixtureTests(DatabaseFixture db) : IClassFixture<DatabaseFixture>
{
    [Fact]
    public void Fixture_IsConnected_WhenCreated()
    {
        db.IsConnected.Should().BeTrue();
        db.ConnectionString.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Fixture_ConnectionString_ContainsServerAddress()
    {
        db.ConnectionString.Should().Contain("Server=");
    }
}
