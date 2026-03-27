using DotNetTraining.Basics.NullableReferenceTypes;
using FluentAssertions;

namespace Basics.Tests;

public class NullableReferenceTypesTests
{
    // ── Null-coalescing ?? ───────────────────────────────────────────────────

    [Fact]
    public void Greet_WithName_ReturnsPersonalGreeting()
    {
        NullSafety.Greet("Alice").Should().Be("Hello, Alice!");
    }

    [Fact]
    public void Greet_WithNull_ReturnsFallback()
    {
        NullSafety.Greet(null).Should().Be("Hello, stranger!");
    }

    // ── Null-conditional ?. ──────────────────────────────────────────────────

    [Fact]
    public void SafeLength_WithValue_ReturnsLength()
    {
        NullSafety.SafeLength("hello").Should().Be(5);
    }

    [Fact]
    public void SafeLength_WithNull_ReturnsNull()
    {
        NullSafety.SafeLength(null).Should().BeNull();
    }

    // ── Chained ?. with ?? ───────────────────────────────────────────────────

    [Fact]
    public void ToUpperOrDefault_WithValue_ReturnsUpperCase()
    {
        NullSafety.ToUpperOrDefault("hello").Should().Be("HELLO");
    }

    [Fact]
    public void ToUpperOrDefault_WithNull_ReturnsFallback()
    {
        NullSafety.ToUpperOrDefault(null, "NONE").Should().Be("NONE");
    }

    // ── Null-coalescing assignment ??= ───────────────────────────────────────

    [Fact]
    public void EnsureNotNull_AssignsDefault_WhenNull()
    {
        string? value = null;
        var result = NullSafety.EnsureNotNull(ref value, "fallback");

        result.Should().Be("fallback");
        value.Should().Be("fallback");
    }

    [Fact]
    public void EnsureNotNull_KeepsExisting_WhenNotNull()
    {
        string? value = "original";
        var result = NullSafety.EnsureNotNull(ref value, "fallback");

        result.Should().Be("original");
        value.Should().Be("original");
    }

    // ── Customer record with nullable fields ─────────────────────────────────

    [Fact]
    public void Customer_PreferredContact_ReturnsEmail_WhenPresent()
    {
        var customer = new Customer { Name = "Alice", Email = "alice@test.com", Phone = "555-1234" };
        customer.PreferredContact.Should().Be("alice@test.com");
    }

    [Fact]
    public void Customer_PreferredContact_FallsToPhone_WhenNoEmail()
    {
        var customer = new Customer { Name = "Bob", Phone = "555-1234" };
        customer.PreferredContact.Should().Be("555-1234");
    }

    [Fact]
    public void Customer_PreferredContact_FallsToDefault_WhenNoContact()
    {
        var customer = new Customer { Name = "Charlie" };
        customer.PreferredContact.Should().Be("no contact info");
    }

    // ── Guard clauses ────────────────────────────────────────────────────────

    [Fact]
    public void ProcessName_ThrowsArgumentNull_WhenNull()
    {
        var act = () => NullGuards.ProcessName(null);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ProcessName_ReturnsTrimmedUpperCase()
    {
        NullGuards.ProcessName("  alice  ").Should().Be("ALICE");
    }

    // ── Pattern matching with null ───────────────────────────────────────────

    [Theory]
    [InlineData(null, "nothing")]
    [InlineData("", "empty string")]
    [InlineData("hi", "string: hi")]
    [InlineData(42, "number: 42")]
    public void Describe_CategorizesValues(object? value, string expected)
    {
        NullGuards.Describe(value).Should().Be(expected);
    }

    // ── Safe dictionary lookup ───────────────────────────────────────────────

    [Fact]
    public void SafeLookup_ReturnsValue_WhenKeyExists()
    {
        var dict = new Dictionary<string, string> { ["key"] = "value" };
        NullGuards.SafeLookup(dict, "key").Should().Be("value");
    }

    [Fact]
    public void SafeLookup_ReturnsNull_WhenKeyMissing()
    {
        var dict = new Dictionary<string, string>();
        NullGuards.SafeLookup(dict, "missing").Should().BeNull();
    }
}
