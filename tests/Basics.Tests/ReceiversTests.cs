using DotNetTraining.Basics.Receivers;
using FluentAssertions;

namespace Basics.Tests;

public class ReceiversTests
{
    [Fact]
    public void Temperature_ToFahrenheit_ConvertsCorrectly()
    {
        var boiling = new Temperature(100);
        boiling.ToFahrenheit().Should().BeApproximately(212.0, 0.01);
    }

    [Fact]
    public void Temperature_Freezing_IsCorrect()
    {
        var freezing = new Temperature(0);
        freezing.ToFahrenheit().Should().BeApproximately(32.0, 0.01);
        freezing.ToKelvin().Should().BeApproximately(273.15, 0.01);
    }

    [Fact]
    public void Temperature_Add_ReturnsNewTemperature()
    {
        var temp = new Temperature(20);
        var warmer = temp.Add(5);
        warmer.Celsius.Should().BeApproximately(25.0, 0.01);
        temp.Celsius.Should().BeApproximately(20.0, 0.01, "struct is immutable — original unchanged");
    }

    [Theory]
    [InlineData("hello world", "Hello World")]
    [InlineData("a", "A")]
    public void ToTitleCase_CapitalizesWords(string input, string expected)
    {
        input.ToTitleCase().Should().Be(expected);
    }

    [Fact]
    public void Truncate_ShortensLongStrings()
    {
        "hello world".Truncate(5).Should().StartWith("hello");
        "hi".Truncate(10).Should().Be("hi");
    }

    [Fact]
    public void Counter_Tracks_Increments()
    {
        var c = new Counter();
        c.Increment();
        c.Increment();
        c.Increment();
        c.Value.Should().Be(3);
    }

    [Fact]
    public void Counter_Reset_SetsValueToZero()
    {
        var c = new Counter();
        c.Increment();
        c.Reset();
        c.Value.Should().Be(0);
        c.IsZero().Should().BeTrue();
    }

    [Fact]
    public void WithIndex_AttachesIndex_ToEachItem()
    {
        string[] items = ["a", "b", "c"];
        var indexed = items.WithIndex().ToList();
        indexed[0].Should().Be((0, "a"));
        indexed[2].Should().Be((2, "c"));
    }
}
