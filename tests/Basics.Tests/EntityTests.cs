using DotNetTraining.Basics.Entity;
using FluentAssertions;

namespace Basics.Tests;

public class EntityTests
{
    // ── record tests ─────────────────────────────────────────────────────────

    [Fact]
    public void Account_Record_Equality_BasedOnValues()
    {
        var a1 = new Account(Guid.Empty, "Alice", 100m);
        var a2 = new Account(Guid.Empty, "Alice", 100m);
        a1.Should().Be(a2, "records use structural equality");
    }

    [Fact]
    public void Account_WithExpression_CreatesModifiedCopy()
    {
        var original = new Account(Guid.NewGuid(), "Alice", 100m);
        var updated = original with { Balance = 200m };

        updated.Balance.Should().Be(200m);
        updated.Owner.Should().Be("Alice");
        original.Balance.Should().Be(100m, "original must be unchanged");
    }

    [Fact]
    public void Account_Deposit_ReturnsNewRecord()
    {
        var account = new Account(Guid.NewGuid(), "Alice", 100m);
        var after = account.Deposit(50m);

        after.Balance.Should().Be(150m);
        account.Balance.Should().Be(100m, "record is immutable");
    }

    // ── class tests ───────────────────────────────────────────────────────────

    [Fact]
    public void BankAccount_Deposit_IncreasesBalance()
    {
        var account = new BankAccount { Owner = "Alice" };
        account.Deposit(100m);
        account.Balance.Should().Be(100m);
    }

    [Fact]
    public void BankAccount_Withdraw_DecreasesBalance()
    {
        var account = new BankAccount { Owner = "Alice" };
        account.Deposit(200m);
        account.Withdraw(50m);
        account.Balance.Should().Be(150m);
    }

    [Fact]
    public void BankAccount_Withdraw_ThrowsWhenInsufficientFunds()
    {
        var account = new BankAccount { Owner = "Alice" };
        account.Deposit(50m);
        var act = () => account.Withdraw(100m);
        act.Should().Throw<InvalidOperationException>();
    }

    // ── struct tests ──────────────────────────────────────────────────────────

    [Fact]
    public void Money_Add_WorksCorrectly()
    {
        var a = new Money(10.50m, "USD");
        var b = new Money(5.25m, "USD");
        var sum = a.Add(b);
        sum.Amount.Should().Be(15.75m);
    }

    [Fact]
    public void Money_Add_ThrowsOnCurrencyMismatch()
    {
        var usd = new Money(10m, "USD");
        var eur = new Money(10m, "EUR");
        var act = () => usd.Add(eur);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Money_Equality_WorksCorrectly()
    {
        var a = new Money(10m, "USD");
        var b = new Money(10m, "USD");
        a.Should().Be(b);
    }

    [Fact]
    public void Coordinate_DistanceTo_CalculatesCorrectly()
    {
        var a = new Coordinate(0, 0);
        var b = new Coordinate(3, 4);
        a.DistanceTo(b).Should().BeApproximately(5.0, 0.001);
    }
}
