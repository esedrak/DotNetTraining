using DotNetTraining.Basics.BackgroundServices;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Basics.Tests;

public class BackgroundServicesTests
{
    [Fact]
    public async Task TickerService_Increments_WhenRunning()
    {
        var service = new TickerService(NullLogger<TickerService>.Instance);
        using var cts = new CancellationTokenSource();

        await service.StartAsync(cts.Token);
        await Task.Delay(350);
        await cts.CancelAsync();

        service.TickCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task TickerService_StopsCleanly_OnCancellation()
    {
        var service = new TickerService(NullLogger<TickerService>.Instance);
        using var cts = new CancellationTokenSource();

        var startTask = service.StartAsync(cts.Token);
        await cts.CancelAsync();

        await startTask.Invoking(t => t).Should().NotThrowAsync();
    }

    [Fact]
    public async Task BackgroundTaskQueue_Enqueue_CanDequeue()
    {
        var queue = new BackgroundTaskQueue();
        Func<CancellationToken, ValueTask> workItem = _ => ValueTask.CompletedTask;

        queue.Enqueue(workItem);
        var dequeued = await queue.DequeueAsync(CancellationToken.None);

        dequeued.Should().BeSameAs(workItem);
    }

    [Fact]
    public async Task QueuedWorkerService_Processes_EnqueuedWork()
    {
        var queue = new BackgroundTaskQueue();
        var flagWasSet = false;

        queue.Enqueue(_ =>
        {
            flagWasSet = true;
            return ValueTask.CompletedTask;
        });

        var service = new QueuedWorkerService(queue, NullLogger<QueuedWorkerService>.Instance);
        using var cts = new CancellationTokenSource();

        await service.StartAsync(cts.Token);
        await Task.Delay(200);
        await cts.CancelAsync();

        flagWasSet.Should().BeTrue();
    }
}
