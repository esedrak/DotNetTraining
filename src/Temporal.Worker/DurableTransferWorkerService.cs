using Bank.Repository;
using Bank.Temporal.Activities;
using Bank.Temporal.Workflows;
using Temporalio.Client;
using Temporalio.Worker;

namespace Temporal.Worker;

/// <summary>
/// Temporal worker for the "durable-transfers" task queue.
/// Registers DurableTransferWorkflow and TransferActivities.
/// </summary>
public class DurableTransferWorkerService(
    IServiceScopeFactory scopeFactory,
    ILogger<DurableTransferWorkerService> logger) : BackgroundService
{
    private const string TaskQueue = "durable-transfers";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Connecting to Temporal at localhost:7233 (durable-transfers)...");

        var client = await TemporalClient.ConnectAsync(new("localhost:7233"));

        // Create a DI scope that lasts for the worker's lifetime.
        // Each activity call shares one BankDbContext — acceptable for a single-process worker.
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBankRepository>();
        var activityLogger = scope.ServiceProvider.GetRequiredService<ILogger<TransferActivities>>();
        var activities = new TransferActivities(repository, activityLogger);

        using var worker = new TemporalWorker(
            client,
            new TemporalWorkerOptions(TaskQueue)
                .AddWorkflow<DurableTransferWorkflow>()
                .AddAllActivities(activities));

        logger.LogInformation("Durable-transfers worker started, listening on task queue '{Queue}'", TaskQueue);

        await worker.ExecuteAsync(stoppingToken);
    }
}
