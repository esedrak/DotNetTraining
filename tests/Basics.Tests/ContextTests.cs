using DotNetTraining.Basics.Context;
using FluentAssertions;

namespace Basics.Tests;

public class ContextTests
{
    [Fact]
    public async Task CountTo_Completes_WhenNotCancelled()
    {
        using var cts = new CancellationTokenSource();
        var result = await CancellationExamples.CountToAsync(5, cts.Token);
        result.Should().Be(5);
    }

    [Fact]
    public async Task CountTo_ThrowsOperationCancelled_WhenCancelledBeforeStart()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await CancellationExamples.CountToAsync(5, cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task TryWorkWithTimeout_ReturnsFalse_WhenWorkExceedsDeadline()
    {
        // 200ms work, 50ms timeout — should timeout
        var result = await CancellationExamples.TryWorkWithTimeoutAsync(workMs: 200);
        result.Should().BeFalse("work takes longer than the timeout");
    }

    [Fact]
    public async Task TryWorkWithTimeout_ReturnsTrue_WhenWorkFinishesInTime()
    {
        // 0ms work, 50ms timeout — should complete
        var result = await CancellationExamples.TryWorkWithTimeoutAsync(workMs: 0);
        result.Should().BeTrue("zero-delay work completes before the 50ms timeout");
    }

    [Fact]
    public async Task LinkedToken_CancelsWhenParentCancels()
    {
        using var parent = new CancellationTokenSource();
        using var child = new CancellationTokenSource();
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(
            parent.Token, child.Token);

        parent.Cancel();
        linked.Token.IsCancellationRequested.Should().BeTrue();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task AsyncLocal_FlowsValueThroughAsyncCalls()
    {
        AsyncLocalExamples.CorrelationId = "request-123";
        await AsyncLocalExamples.SimulateRequestAsync("request-123");
        AsyncLocalExamples.CorrelationId.Should().Be("request-123");
    }
}
