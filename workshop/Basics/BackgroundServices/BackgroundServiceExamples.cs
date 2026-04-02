using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotNetTraining.Basics.BackgroundServices;

// ── TickerService ─────────────────────────────────────────────────────────────

/// <summary>
/// A simple periodic background service that increments a counter every 100 ms.
/// Demonstrates the <see cref="BackgroundService"/> base class and how to loop
/// until the cancellation token fires.
/// </summary>
public class TickerService(ILogger<TickerService> logger) : BackgroundService
{
    private int _tickCount;

    /// <summary>Number of ticks elapsed since the service started.</summary>
    public int TickCount => _tickCount;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                int current = Interlocked.Increment(ref _tickCount);
                logger.LogInformation("Tick {Count}", current);
                await Task.Delay(100, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown — cancellation is expected, not an error.
        }
    }
}

// ── IBackgroundTaskQueue ──────────────────────────────────────────────────────

/// <summary>
/// Abstraction for an async work queue consumed by <see cref="QueuedWorkerService"/>.
/// </summary>
public interface IBackgroundTaskQueue
{
    /// <summary>Enqueue a unit of work to be executed on the background thread.</summary>
    void Enqueue(Func<CancellationToken, ValueTask> workItem);

    /// <summary>Dequeue the next work item, waiting asynchronously until one is available.</summary>
    ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken ct);
}

// ── BackgroundTaskQueue ───────────────────────────────────────────────────────

/// <summary>
/// <see cref="Channel{T}"/>-backed implementation of <see cref="IBackgroundTaskQueue"/>.
/// An unbounded channel provides a lock-free FIFO queue.
/// The writer side is used by producers; the reader side is drained by <see cref="QueuedWorkerService"/>.
/// </summary>
public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Func<CancellationToken, ValueTask>> _channel =
        Channel.CreateUnbounded<Func<CancellationToken, ValueTask>>();

    /// <inheritdoc/>
    public void Enqueue(Func<CancellationToken, ValueTask> workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem);
        _channel.Writer.TryWrite(workItem);
    }

    /// <inheritdoc/>
    public ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken ct) =>
        _channel.Reader.ReadAsync(ct);
}

// ── QueuedWorkerService ───────────────────────────────────────────────────────

/// <summary>
/// A hosted service that drains <see cref="IBackgroundTaskQueue"/> in a continuous loop.
/// Work items are executed sequentially; exceptions are logged and processing continues.
/// </summary>
public class QueuedWorkerService(
    IBackgroundTaskQueue queue,
    ILogger<QueuedWorkerService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Func<CancellationToken, ValueTask> workItem;

            try
            {
                workItem = await queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Host is shutting down — exit the loop cleanly.
                break;
            }

            try
            {
                await workItem(stoppingToken);
            }
            catch (Exception ex)
            {
                // Log the error but keep processing subsequent items.
                logger.LogError(ex, "Error occurred while executing background work item");
            }
        }
    }
}

// ── Registration extension ────────────────────────────────────────────────────

public static class HostBuilderExtensions
{
    /// <summary>
    /// Registers <see cref="TickerService"/>, <see cref="BackgroundTaskQueue"/> (singleton),
    /// and <see cref="QueuedWorkerService"/> as hosted services.
    /// </summary>
    public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<TickerService>();
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddHostedService<QueuedWorkerService>();
        return services;
    }
}
