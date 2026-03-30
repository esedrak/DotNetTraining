using Temporal.Workflows.Activities;
using Temporal.Workflows.Workflows;
using Temporalio.Client;
using Temporalio.Worker;

namespace Temporal.Worker;

/// <summary>
/// Temporal worker — polls the Temporal server for tasks to execute.
/// </summary>
public class TemporalWorkerService(ILogger<TemporalWorkerService> logger) : BackgroundService
{
    private const string TaskQueue = "order-processing";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Connecting to Temporal at localhost:7233...");

        var client = await TemporalClient.ConnectAsync(new("localhost:7233"));

        using var worker = new TemporalWorker(client, new TemporalWorkerOptions(TaskQueue)
            .AddWorkflow<OrderWorkflow>()
            .AddWorkflow<PaymentWorkflow>()
            .AddAllActivities(new OrderActivities(logger.As<OrderActivities>())));

        logger.LogInformation("Worker started, listening on task queue '{Queue}'", TaskQueue);

        await worker.ExecuteAsync(stoppingToken);
    }
}

// Extension to create a typed logger from a generic one
file static class LoggerExtensions
{
    public static ILogger<T> As<T>(this ILogger logger) =>
        Microsoft.Extensions.Logging.LoggerFactory
            .Create(b => b.AddConsole())
            .CreateLogger<T>();
}
