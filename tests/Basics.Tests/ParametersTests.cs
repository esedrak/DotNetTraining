using DotNetTraining.Basics.Parameters;
using FluentAssertions;

namespace Basics.Tests;

public class ParametersTests
{
    [Fact]
    public void IncrementValue_DoesNotModifyOriginal()
    {
        int x = 10;
        var result = ParameterExamples.IncrementValue(x);
        result.Should().Be(11);
        x.Should().Be(10, "value parameter — caller's variable unchanged");
    }

    [Fact]
    public void IncrementRef_ModifiesVariable()
    {
        int x = 10;
        ParameterExamples.IncrementRef(ref x);
        x.Should().Be(11);
    }

    [Fact]
    public void TryParsePositive_ReturnsTrue_ForPositiveInput()
    {
        var success = ParameterExamples.TryParsePositive("42", out int result);
        success.Should().BeTrue();
        result.Should().Be(42);
    }

    [Fact]
    public void TryParsePositive_ReturnsFalse_ForNonNumeric()
    {
        var success = ParameterExamples.TryParsePositive("abc", out int result);
        success.Should().BeFalse();
        result.Should().Be(0);
    }

    [Fact]
    public void TryParsePositive_ReturnsFalse_ForNegative()
    {
        var success = ParameterExamples.TryParsePositive("-5", out int result);
        success.Should().BeFalse();
        result.Should().Be(0);
    }

    [Fact]
    public void Sum_WithParams_AddsAllValues()
    {
        ParameterExamples.Sum(1, 2, 3, 4, 5).Should().Be(15);
        ParameterExamples.Sum().Should().Be(0);
    }

    [Fact]
    public void FormatName_UsesDefaultSeparator()
    {
        ParameterExamples.FormatName("Alice", "Smith").Should().Be("Alice Smith");
    }

    [Fact]
    public void FormatName_WithUpperCase_IsUpperCased()
    {
        ParameterExamples.FormatName("alice", "smith", upperCase: true)
            .Should().Be("ALICE SMITH");
    }

    [Fact]
    public void DistanceBetween_CalculatesPythagorean()
    {
        // 3-4-5 right triangle — in parameters require named variables
        (double X, double Y) a = (3.0, 0.0);
        (double X, double Y) b = (0.0, 4.0);
        ParameterExamples.DistanceBetween(in a, in b)
            .Should().BeApproximately(5.0, 0.001);
    }
}
