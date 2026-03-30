using DotNetTraining.Challenges.Basics.ImplMe;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;

namespace Basics.Tests;

/// <summary>
/// Tests for the ImplMe challenges.
/// Run with: dotnet test tests/Basics.Tests --filter "FullyQualifiedName~ChallengeImplMe"
/// </summary>

// ── Challenge 3: LINQ banking queries ────────────────────────────────────────

public class ChallengeImplMe_LinqTests
{
    private static readonly List<BankTransaction> Transactions =
    [
        new("Alice", 500m, "Deposit"),
        new("Bob", -200m, "Withdrawal"),
        new("Alice", -100m, "Withdrawal"),
        new("Charlie", 1000m, "Deposit"),
        new("Bob", 300m, "Deposit"),
    ];

    [Fact]
    public void GetWithdrawals_ReturnsNegativeAmounts()
    {
        var withdrawals = LinqChallenge.GetWithdrawals(Transactions).ToList();

        withdrawals.Should().HaveCount(2);
        withdrawals.Should().AllSatisfy(t => t.Amount.Should().BeNegative());
    }

    [Fact]
    public void TotalByOwner_GroupsAndSums()
    {
        var totals = LinqChallenge.TotalByOwner(Transactions);

        totals.Should().HaveCount(3);
        totals["Alice"].Should().Be(400m);
        totals["Bob"].Should().Be(100m);
        totals["Charlie"].Should().Be(1000m);
    }

    [Fact]
    public void LargestByAbsoluteAmount_FindsMax()
    {
        var largest = LinqChallenge.LargestByAbsoluteAmount(Transactions);

        largest.Should().NotBeNull();
        largest!.Owner.Should().Be("Charlie");
        largest.Amount.Should().Be(1000m);
    }

    [Fact]
    public void LargestByAbsoluteAmount_ReturnsNull_WhenEmpty()
    {
        LinqChallenge.LargestByAbsoluteAmount([]).Should().BeNull();
    }
}
