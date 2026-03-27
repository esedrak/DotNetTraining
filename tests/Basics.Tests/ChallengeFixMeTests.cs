using DotNetTraining.Challenges.Basics.FixMe;
using FluentAssertions;

namespace Basics.Tests;

/// <summary>
/// Tests that demonstrate the bugs in the FixMe challenges.
/// These tests document the EXPECTED buggy behavior.
/// After fixing the bugs, update the tests to verify correct behavior.
/// </summary>
public class ChallengeFixMeTests
{
    // ── Challenge 6: Struct mutation ─────────────────────────────────────────

    [Fact]
    public void StructMutation_DepositIsLost_DueToCopySemantics()
    {
        var bug = new StructMutationBug();
        bug.AddAccount("Alice", 100m);

        bug.DepositToFirst(50m);

        // BUG: Balance is still 100 because DepositToFirst mutates a copy
        bug.GetFirstBalance().Should().Be(100m,
            "struct copy semantics means the deposit was applied to a copy, not the original");
    }

    // ── Challenge 7: Disposable leak ─────────────────────────────────────────

    [Fact]
    public void DisposableLeak_ResourcesNotDisposed()
    {
        DisposableLeakBug.DisposalLog.Clear();
        var bug = new DisposableLeakBug();

        var results = bug.ProcessMany(["a", "b", "c"]);

        results.Should().HaveCount(3);
        // BUG: None of the resources were disposed
        DisposableLeakBug.DisposalLog.Should().BeEmpty(
            "resources were never disposed — this would exhaust a connection pool in production");
    }

    // ── Challenge 8: async void ──────────────────────────────────────────────

    [Fact]
    public async Task AsyncVoid_ExceptionIsUnobservable()
    {
        var bug = new AsyncVoidBug();

        // async void fire-and-forget — we can't await it or catch its exceptions
        bug.ProcessItemAsync("good");
        await Task.Delay(50); // Give it time to complete

        bug.Log.Should().Contain("good");

        // The "bad" item throws, but since it's async void,
        // the exception is raised on the thread pool and is unobservable by the caller.
        // In production, this would crash the process or silently fail.
    }
}
