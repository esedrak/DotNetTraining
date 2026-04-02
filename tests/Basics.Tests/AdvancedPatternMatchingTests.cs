using DotNetTraining.Basics.AdvancedPatternMatching;
using FluentAssertions;

namespace Basics.Tests;

public class AdvancedPatternMatchingTests
{
    // ── Relational patterns ───────────────────────────────────────────────────

    [Theory]
    [InlineData(-1, "invalid")]
    [InlineData(50, "fail")]
    [InlineData(70, "pass")]
    [InlineData(85, "merit")]
    [InlineData(95, "distinction")]
    [InlineData(101, "invalid")]
    public void RangePatterns_Classify_ReturnsExpected(int score, string expected)
    {
        RangePatterns.Classify(score).Should().Be(expected);
    }

    [Theory]
    [InlineData(-5.0, "freezing")]
    [InlineData(10.0, "cold")]
    [InlineData(20.0, "mild")]
    [InlineData(30.0, "hot")]
    public void RangePatterns_Temperature_ReturnsExpected(double celsius, string expected)
    {
        RangePatterns.Temperature(celsius).Should().Be(expected);
    }

    // ── Logical patterns ──────────────────────────────────────────────────────

    [Fact]
    public void LogicalPatterns_IsWeekend_True_ForSaturday()
    {
        LogicalPatterns.IsWeekend(DayOfWeek.Saturday).Should().BeTrue();
        LogicalPatterns.IsWeekend(DayOfWeek.Sunday).Should().BeTrue();
    }

    [Fact]
    public void LogicalPatterns_IsWeekend_False_ForMonday()
    {
        LogicalPatterns.IsWeekend(DayOfWeek.Monday).Should().BeFalse();
    }

    [Fact]
    public void LogicalPatterns_IsWorkingHour_True_AtNoon()
    {
        LogicalPatterns.IsWorkingHour(12).Should().BeTrue();
    }

    [Fact]
    public void LogicalPatterns_IsWorkingHour_False_AtMidnight()
    {
        LogicalPatterns.IsWorkingHour(0).Should().BeFalse();
    }

    // ── Property patterns ─────────────────────────────────────────────────────

    [Fact]
    public void PropertyPatterns_LivesInLondon_True()
    {
        var p = new Person("Alice", 30, new Address("Baker St", "London", "UK"));
        PropertyPatterns.LivesInLondon(p).Should().BeTrue();
    }

    [Fact]
    public void PropertyPatterns_LivesInLondon_False()
    {
        var p = new Person("Bob", 30, new Address("Main St", "Paris", "FR"));
        PropertyPatterns.LivesInLondon(p).Should().BeFalse();
    }

    [Theory]
    [InlineData(10, "US", "minor")]
    [InlineData(70, "UK", "senior")]
    [InlineData(30, "US", "US adult")]
    [InlineData(40, "DE", "adult")]
    public void PropertyPatterns_Describe_ReturnsExpected(int age, string country, string expected)
    {
        var p = new Person("X", age, new Address("", "", country));
        PropertyPatterns.Describe(p).Should().Be(expected);
    }

    // ── Positional patterns ───────────────────────────────────────────────────

    [Theory]
    [InlineData(0, 0, "origin")]
    [InlineData(1, 1, "Q1")]
    [InlineData(-1, 1, "Q2")]
    [InlineData(-1, -1, "Q3")]
    [InlineData(1, -1, "Q4")]
    [InlineData(0, 5, "axis")]
    public void PositionalPatterns_Quadrant_ReturnsExpected(int x, int y, string expected)
    {
        PositionalPatterns.Quadrant(new Point(x, y)).Should().Be(expected);
    }

    // ── List patterns ─────────────────────────────────────────────────────────

    [Theory]
    [InlineData(new int[] { }, "empty")]
    [InlineData(new int[] { 7 }, "one element: 7")]
    [InlineData(new int[] { 1, 2 }, "two elements: 1, 2")]
    [InlineData(new int[] { 1, 2, 3 }, "starts 1, ends 3")]
    public void ListPatterns_DescribeList_ReturnsExpected(int[] values, string expected)
    {
        ListPatterns.DescribeList(values).Should().Be(expected);
    }

    [Fact]
    public void ListPatterns_StartsWithOne_True()
    {
        ListPatterns.StartsWithOne([1, 2, 3]).Should().BeTrue();
    }

    [Fact]
    public void ListPatterns_StartsWithOne_False()
    {
        ListPatterns.StartsWithOne([2, 3]).Should().BeFalse();
    }

    [Fact]
    public void ListPatterns_HasExactlyThree_True()
    {
        ListPatterns.HasExactlyThree([1, 2, 3]).Should().BeTrue();
    }

    [Fact]
    public void ListPatterns_HasExactlyThree_False()
    {
        ListPatterns.HasExactlyThree([1, 2]).Should().BeFalse();
    }

    // ── Guard / property pattern switch ──────────────────────────────────────

    [Fact]
    public void GuardPatterns_CalculateDiscount_Cancelled_IsZero()
    {
        var order = new Order(1500m, "cancelled", true);
        GuardPatterns.CalculateDiscount(order).Should().Be(0m);
    }

    [Fact]
    public void GuardPatterns_CalculateDiscount_HighAmountPriority_Is15Percent()
    {
        var order = new Order(2000m, "active", true);
        GuardPatterns.CalculateDiscount(order).Should().Be(300m);
    }

    [Fact]
    public void GuardPatterns_CalculateDiscount_HighAmount_Is10Percent()
    {
        var order = new Order(2000m, "active", false);
        GuardPatterns.CalculateDiscount(order).Should().Be(200m);
    }

    [Fact]
    public void GuardPatterns_CalculateDiscount_PriorityOnly_Is5Percent()
    {
        var order = new Order(500m, "active", true);
        GuardPatterns.CalculateDiscount(order).Should().Be(25m);
    }

    [Fact]
    public void GuardPatterns_CalculateDiscount_Default_IsZero()
    {
        var order = new Order(100m, "active", false);
        GuardPatterns.CalculateDiscount(order).Should().Be(0m);
    }
}
