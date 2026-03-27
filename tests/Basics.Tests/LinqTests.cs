using DotNetTraining.Basics.Linq;
using FluentAssertions;

namespace Basics.Tests;

public class LinqTests
{
    private static readonly List<Transaction> Transactions =
    [
        new("Alice", "Deposit", 500m, DateTimeOffset.UtcNow.AddDays(-5)),
        new("Bob", "Deposit", 300m, DateTimeOffset.UtcNow.AddDays(-4)),
        new("Alice", "Withdrawal", -200m, DateTimeOffset.UtcNow.AddDays(-3)),
        new("Charlie", "Deposit", 1000m, DateTimeOffset.UtcNow.AddDays(-2)),
        new("Bob", "Withdrawal", -450m, DateTimeOffset.UtcNow.AddDays(-1)),
    ];

    private static readonly List<AccountSummary> Accounts =
    [
        new("Alice", 300m),
        new("Bob", -150m),
        new("Charlie", 1000m),
        new("Diana", -50m),
    ];

    // ── Filtering ────────────────────────────────────────────────────────────

    [Fact]
    public void GetOverdrawnAccounts_FiltersNegativeBalances()
    {
        var overdrawn = LinqExamples.GetOverdrawnAccounts(Accounts).ToList();

        overdrawn.Should().HaveCount(2);
        overdrawn.Should().AllSatisfy(a => a.Balance.Should().BeNegative());
        overdrawn.Select(a => a.Owner).Should().BeEquivalentTo(["Bob", "Diana"]);
    }

    // ── Projection + Distinct ────────────────────────────────────────────────

    [Fact]
    public void GetDistinctOwners_ReturnsUniqueNames()
    {
        var owners = LinqExamples.GetDistinctOwners(Transactions).ToList();

        owners.Should().HaveCount(3);
        owners.Should().BeEquivalentTo(["Alice", "Bob", "Charlie"]);
    }

    // ── Aggregation ──────────────────────────────────────────────────────────

    [Fact]
    public void TotalBalance_SumsAllAccounts()
    {
        var total = LinqExamples.TotalBalance(Accounts);

        total.Should().Be(1100m); // 300 + (-150) + 1000 + (-50)
    }

    // ── GroupBy ──────────────────────────────────────────────────────────────

    [Fact]
    public void SumByOwner_GroupsAndAggregates()
    {
        var sums = LinqExamples.SumByOwner(Transactions).ToList();

        sums.Should().HaveCount(3);
        sums.Should().Contain(s => s.Owner == "Alice" && s.Balance == 300m);
        sums.Should().Contain(s => s.Owner == "Bob" && s.Balance == -150m);
        sums.Should().Contain(s => s.Owner == "Charlie" && s.Balance == 1000m);
    }

    [Fact]
    public void SumByOwnerQuery_MatchesMethodSyntax()
    {
        var methodResult = LinqExamples.SumByOwner(Transactions).OrderBy(s => s.Owner).ToList();
        var queryResult = LinqExamples.SumByOwnerQuery(Transactions).OrderBy(s => s.Owner).ToList();

        queryResult.Should().BeEquivalentTo(methodResult);
    }

    // ── MaxBy ────────────────────────────────────────────────────────────────

    [Fact]
    public void LargestTransaction_FindsMaxAbsoluteAmount()
    {
        var largest = LinqExamples.LargestTransaction(Transactions);

        largest.Should().NotBeNull();
        largest!.Owner.Should().Be("Charlie");
        largest.Amount.Should().Be(1000m);
    }

    [Fact]
    public void LargestTransaction_ReturnsNull_WhenEmpty()
    {
        var largest = LinqExamples.LargestTransaction([]);
        largest.Should().BeNull();
    }

    // ── OrderBy + Take ───────────────────────────────────────────────────────

    [Fact]
    public void TopTransactions_ReturnsDescendingByAmount()
    {
        var top = LinqExamples.TopTransactions(Transactions, 2).ToList();

        top.Should().HaveCount(2);
        top[0].Amount.Should().Be(1000m);
        top[1].Amount.Should().Be(500m);
    }

    // ── Any ──────────────────────────────────────────────────────────────────

    [Fact]
    public void AnyOverdrawn_ReturnsTrueWhenNegativeExists()
    {
        LinqExamples.AnyOverdrawn(Accounts).Should().BeTrue();
    }

    [Fact]
    public void AnyOverdrawn_ReturnsFalseWhenAllPositive()
    {
        var healthy = Accounts.Where(a => a.Balance >= 0);
        LinqExamples.AnyOverdrawn(healthy).Should().BeFalse();
    }

    // ── Deferred execution ───────────────────────────────────────────────────

    [Fact]
    public void DeferredFilter_DoesNotExecuteUntilEnumerated()
    {
        var source = new List<Transaction>(Transactions);
        var query = LinqExamples.DeferredFilter(source, 500m);

        // Add another transaction AFTER creating the query
        source.Add(new Transaction("Eve", "Big deposit", 999m, DateTimeOffset.UtcNow));

        // The query sees the new item because it wasn't evaluated yet
        var results = query.ToList();
        results.Should().Contain(t => t.Owner == "Eve");
    }
}
