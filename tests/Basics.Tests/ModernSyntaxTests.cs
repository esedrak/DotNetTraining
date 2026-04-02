using DotNetTraining.Basics.ModernSyntax;
using FluentAssertions;

namespace Basics.Tests;

public class ModernSyntaxTests
{
    // ── Primary constructors ──────────────────────────────────────────────────

    [Fact]
    public void PrimaryConstructor_Point_ExposesProperties()
    {
        var point = new Point(3, 4);
        point.X.Should().Be(3);
        point.Y.Should().Be(4);
    }

    [Fact]
    public void PrimaryConstructor_Point_DistanceTo()
    {
        var origin = new Point(0, 0);
        var p = new Point(3, 4);

        origin.DistanceTo(p).Should().BeApproximately(5.0, 0.0001);
    }

    [Fact]
    public void PrimaryConstructor_Size_AreaIsCorrect()
    {
        var size = new Size(4, 5);
        size.Area.Should().Be(20);
    }

    // ── Collection expressions ────────────────────────────────────────────────

    [Fact]
    public void CollectionExpression_GetPrimes_HasFiveElements()
    {
        CollectionExpressions.GetPrimes().Should().HaveCount(5);
    }

    [Fact]
    public void CollectionExpression_GetColours_ContainsRed()
    {
        CollectionExpressions.GetColours().Should().Contain("red");
    }

    [Fact]
    public void CollectionExpression_Combine_MergesArrays()
    {
        var result = CollectionExpressions.Combine([1, 2], [3, 4]);
        result.Should().Equal(1, 2, 3, 4);
    }

    [Fact]
    public void CollectionExpression_Empty_IsEmptyArray()
    {
        CollectionExpressions.Empty().Should().BeEmpty();
    }

    // ── with expressions ──────────────────────────────────────────────────────

    [Fact]
    public void WithExpression_UpdateEmail_ChangesEmail()
    {
        var original = new Customer("Alice", "alice@old.com",
            new Address("1 Main St", "London", "W1A"));
        var updated = WithExpressionExamples.UpdateEmail(original, "alice@new.com");

        updated.Email.Should().Be("alice@new.com");
        original.Email.Should().Be("alice@old.com", "original is unchanged");
    }

    [Fact]
    public void WithExpression_MoveCity_ChangesNestedCity()
    {
        var original = new Customer("Bob", "bob@example.com",
            new Address("10 Park Lane", "Manchester", "M1"));
        var moved = WithExpressionExamples.MoveCity(original, "Leeds");

        moved.Address.City.Should().Be("Leeds");
        original.Address.City.Should().Be("Manchester", "original is unchanged");
    }

    // ── Record value equality ─────────────────────────────────────────────────

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var a = new Money(10m, "USD");
        var b = new Money(10m, "USD");

        RecordEqualityExamples.AreEqual(a, b).Should().BeTrue();
    }

    [Fact]
    public void RecordEquality_DifferentValues_AreNotEqual()
    {
        var a = new Money(10m, "USD");
        var b = new Money(20m, "USD");

        RecordEqualityExamples.AreEqual(a, b).Should().BeFalse();
    }

    [Fact]
    public void RecordDeconstruct_ExtractsValues()
    {
        var money = new Money(5m, "GBP");
        var (amount, currency) = RecordEqualityExamples.Unpack(money);

        amount.Should().Be(5m);
        currency.Should().Be("GBP");
    }

    // ── required / init properties ────────────────────────────────────────────

    [Fact]
    public void RequiredProperties_CanBeSetAtInit()
    {
        var profile = new UserProfile
        {
            Username = "alice",
            Email = "alice@example.com",
            DisplayName = "Alice"
        };

        profile.Username.Should().Be("alice");
        profile.Email.Should().Be("alice@example.com");
    }

    [Fact]
    public void ApiKey_CustomConstructor_SetsRequiredMembers()
    {
        var key = new ApiKey("abc123", "payments");

        key.Key.Should().Be("abc123");
        key.Service.Should().Be("payments");
        key.IssuedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }
}
