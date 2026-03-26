using DotNetTraining.Basics.Generics;
using FluentAssertions;

namespace Basics.Tests;

public class GenericsTests
{
    [Theory]
    [InlineData(3, 5, 3)]
    [InlineData(10, 2, 2)]
    [InlineData(0, 0, 0)]
    public void Min_ReturnsSmaller(int a, int b, int expected)
    {
        GenericMethods.Min(a, b).Should().Be(expected);
    }

    [Fact]
    public void GetDefault_ReturnsDefaultForValueType()
    {
        GenericMethods.GetDefault<int>().Should().Be(0);
        GenericMethods.GetDefault<bool>().Should().BeFalse();
    }

    [Fact]
    public void GetDefault_ReturnsNullForReferenceType()
    {
        GenericMethods.GetDefault<string>().Should().BeNull();
    }

    [Fact]
    public void Swap_ExchangesTwoValues()
    {
        int a = 1, b = 2;
        GenericMethods.Swap(ref a, ref b);
        a.Should().Be(2);
        b.Should().Be(1);
    }

    [Fact]
    public void Where_FiltersSequence_Correctly()
    {
        var evens = GenericMethods.Where([1, 2, 3, 4, 5], x => x % 2 == 0).ToList();
        evens.Should().BeEquivalentTo([2, 4]);
    }

    [Fact]
    public void Stack_PushAndPop_WorksLIFO()
    {
        var stack = new GenericStack<int>();
        stack.Push(1);
        stack.Push(2);
        stack.Push(3);

        stack.Pop().Should().Be(3);
        stack.Pop().Should().Be(2);
        stack.Count.Should().Be(1);
    }

    [Fact]
    public void Stack_Pop_ThrowsOnEmpty()
    {
        var stack = new GenericStack<string>();
        var act = () => stack.Pop();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Stack_IsEmpty_ReturnsTrueWhenEmpty()
    {
        var stack = new GenericStack<double>();
        stack.IsEmpty.Should().BeTrue();
        stack.Push(1.0);
        stack.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void Optional_HasValue_WhenCreatedWithValue()
    {
        var opt = Optional<int>.Some(42);
        opt.HasValue.Should().BeTrue();
        opt.Value.Should().Be(42);
    }

    [Fact]
    public void Optional_IsEmpty_WhenNone()
    {
        var opt = Optional<string>.None();
        opt.HasValue.Should().BeFalse();
    }

    [Fact]
    public void Optional_Value_ThrowsWhenNone()
    {
        var opt = Optional<int>.None();
        var act = () => opt.Value;
        act.Should().Throw<InvalidOperationException>();
    }
}
