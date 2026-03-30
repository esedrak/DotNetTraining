using DotNetTraining.Challenges.Basics.ImplMe;
using FluentAssertions;

namespace Basics.Tests;

/// <summary>
/// Tests for the ImplMe challenges. These tests will FAIL until you implement the stubs.
/// Run with: dotnet test tests/Basics.Tests --filter "FullyQualifiedName~ChallengeImplMe"
/// </summary>
public class ChallengeImplMeTests
{
    // ── Challenge 4: LINQ banking queries ────────────────────────────────────

    private static readonly List<BankTransaction> Transactions =
    [
        new("Alice", 500m, "Deposit"),
        new("Bob", -200m, "Withdrawal"),
        new("Alice", -100m, "Withdrawal"),
        new("Charlie", 1000m, "Deposit"),
        new("Bob", 300m, "Deposit"),
    ];

    [Fact(Skip = "Not yet implemented — remove Skip= when complete")]
    public void GetWithdrawals_ReturnsNegativeAmounts()
    {
        var withdrawals = LinqChallenge.GetWithdrawals(Transactions).ToList();

        withdrawals.Should().HaveCount(2);
        withdrawals.Should().AllSatisfy(t => t.Amount.Should().BeNegative());
    }

    [Fact(Skip = "Not yet implemented — remove Skip= when complete")]
    public void TotalByOwner_GroupsAndSums()
    {
        var totals = LinqChallenge.TotalByOwner(Transactions);

        totals.Should().HaveCount(3);
        totals["Alice"].Should().Be(400m);
        totals["Bob"].Should().Be(100m);
        totals["Charlie"].Should().Be(1000m);
    }

    [Fact(Skip = "Not yet implemented — remove Skip= when complete")]
    public void LargestByAbsoluteAmount_FindsMax()
    {
        var largest = LinqChallenge.LargestByAbsoluteAmount(Transactions);

        largest.Should().NotBeNull();
        largest!.Owner.Should().Be("Charlie");
        largest.Amount.Should().Be(1000m);
    }

    [Fact(Skip = "Not yet implemented — remove Skip= when complete")]
    public void LargestByAbsoluteAmount_ReturnsNull_WhenEmpty()
    {
        LinqChallenge.LargestByAbsoluteAmount([]).Should().BeNull();
    }

    // ── Challenge 5: Result<T> pattern ───────────────────────────────────────

    [Fact(Skip = "Not yet implemented — remove Skip= when complete")]
    public void TryCatch_ReturnsSuccess_WhenNoException()
    {
        var result = ResultExtensions.TryCatch(() => 42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact(Skip = "Not yet implemented — remove Skip= when complete")]
    public void TryCatch_ReturnsFailure_WhenExceptionThrown()
    {
        var result = ResultExtensions.TryCatch<int>(() => throw new InvalidOperationException("boom"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("boom");
    }

    [Fact(Skip = "Not yet implemented — remove Skip= when complete")]
    public void Map_TransformsSuccessValue()
    {
        var result = Result<int>.Success(5);
        var mapped = ResultExtensions.Map(result, x => x * 2);

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(10);
    }

    [Fact(Skip = "Not yet implemented — remove Skip= when complete")]
    public void Map_PropagatesFailure()
    {
        var result = Result<int>.Failure("error");
        var mapped = ResultExtensions.Map(result, x => x * 2);

        mapped.IsSuccess.Should().BeFalse();
        mapped.Error.Should().Be("error");
    }
}
