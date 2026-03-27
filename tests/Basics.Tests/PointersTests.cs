using DotNetTraining.Basics.ValueAndReferenceTypes;
using FluentAssertions;

namespace Basics.Tests;

public class PointersTests
{
    [Fact]
    public void IncrementValue_DoesNotModifyCallerVariable()
    {
        int x = 5;
        var result = RefOutExamples.IncrementValue(x);

        result.Should().Be(6);
        x.Should().Be(5, "IncrementValue passes by value — caller is not modified");
    }

    [Fact]
    public void IncrementRef_ModifiesCallerVariable()
    {
        int x = 5;
        RefOutExamples.IncrementRef(ref x);

        x.Should().Be(6, "IncrementRef passes by reference — caller IS modified");
    }

    [Theory]
    [InlineData("42", true, 84)]
    [InlineData("-1", false, 0)]
    [InlineData("abc", false, 0)]
    public void TryDouble_ReturnsExpectedResult(string input, bool expectedSuccess, int expectedResult)
    {
        var success = RefOutExamples.TryDouble(input, out int result);

        success.Should().Be(expectedSuccess);
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void Counter_TracksIncrements()
    {
        var counter = new Counter();
        counter.Value.Should().Be(0);

        counter.Increment();
        counter.Increment();
        counter.Increment();

        counter.Value.Should().Be(3);
    }

    [Fact]
    public void StructCopy_DoesNotAffectOriginal()
    {
        // Struct assignment creates a copy (value semantics)
        var original = new Point { X = 1, Y = 2 };
        var copy = original;
        copy.X = 99;

        original.X.Should().Be(1, "struct copy should not affect original");
    }

    [Fact]
    public void ClassReference_AffectsSharedObject()
    {
        // Class assignment copies the reference (reference semantics)
        var original = new PointClass { X = 1, Y = 2 };
        var alias = original;
        alias.X = 99;

        original.X.Should().Be(99, "class references share the same object");
    }

    [Fact]
    public void SpanSum_CalculatesCorrectly()
    {
        int[] numbers = [1, 2, 3, 4, 5];
        SpanExamples.Sum(numbers).Should().Be(15);
    }

    [Fact]
    public void NullableInt_CanBeNull()
    {
        var result = NullableExamples.FindFirst([1, 2, 3], x => x > 10);
        result.Should().BeNull("no element matches the predicate");

        var found = NullableExamples.FindFirst([1, 2, 3], x => x == 2);
        found.Should().HaveValue().And.Be(2);
    }
}
