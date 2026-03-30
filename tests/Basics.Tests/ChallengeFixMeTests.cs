using DotNetTraining.Challenges.Basics.FixMe;
using FluentAssertions;

namespace Basics.Tests;

public class ChallengeFixMeTests
{
    // ── Challenge 6: Struct mutation ─────────────────────────────────────────

    [Fact]
    public void StructMutation_DepositIsApplied()
    {
        var fix = new StructMutationBug();
        fix.AddAccount("Alice", 100m);

        fix.DepositToFirst(50m);

        fix.GetFirstBalance().Should().Be(150m);
    }

    // ── Challenge 7: Disposable leak ─────────────────────────────────────────

    [Fact]
    public void DisposableLeak_AllResourcesAreDisposed()
    {
        DisposableLeakBug.DisposalLog.Clear();
        var fix = new DisposableLeakBug();

        var results = fix.ProcessMany(["a", "b", "c"]);

        results.Should().HaveCount(3);
        DisposableLeakBug.DisposalLog.Should().BeEquivalentTo(["a", "b", "c"]);
    }

    // ── Challenge 8: async void ──────────────────────────────────────────────

    [Fact]
    public async Task AsyncTask_ExceptionIsObservable()
    {
        var fix = new AsyncVoidBug();

        await fix.ProcessItemAsync("good");
        fix.Log.Should().Contain("good");

        await FluentActions.Awaiting(() => fix.ProcessItemAsync("bad"))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Processing failed!");
    }
}
