using Temporalio.Workflows;
using Temporal.Domain;
using Temporal.Workflows.Activities;

namespace Temporal.Workflows.Workflows;

/// <summary>
/// Order processing workflow.
/// Equivalent to the OrderWorkflow in GoTraining's Temporal module.
///
/// A workflow orchestrates activities (steps) in a durable, fault-tolerant way.
/// If the worker crashes, Temporal replays the workflow history to resume from where it left off.
/// </summary>
[Workflow]
public class OrderWorkflow
{
    private string? _cancellationReason;

    [WorkflowRun]
    public async Task<Order> RunAsync(Order order)
    {
        // Validate the order
        order = await Workflow.ExecuteActivityAsync(
            (OrderActivities a) => a.ValidateOrderAsync(order),
            new ActivityOptions { StartToCloseTimeout = TimeSpan.FromSeconds(30) });

        // Check for cancellation before continuing
        if (_cancellationReason is not null)
        {
            order.Status = OrderStatus.Cancelled;
            order.FailureReason = _cancellationReason;
            return order;
        }

        // Process payment via child workflow
        order = await Workflow.ExecuteChildWorkflowAsync(
            (PaymentWorkflow w) => w.RunAsync(order),
            new ChildWorkflowOptions { Id = $"payment-{order.Id}" });

        if (order.Status == OrderStatus.PaymentFailed)
        {
            order.Status = OrderStatus.Cancelled;
            order.FailureReason = "Payment failed";
            return order;
        }

        // Pick and ship
        order = await Workflow.ExecuteActivityAsync(
            (OrderActivities a) => a.PickOrderAsync(order),
            new ActivityOptions
            {
                StartToCloseTimeout = TimeSpan.FromMinutes(5),
                RetryPolicy = new() { MaximumAttempts = 3 }
            });

        order = await Workflow.ExecuteActivityAsync(
            (OrderActivities a) => a.ShipOrderAsync(order),
            new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(2) });

        order.Status = OrderStatus.Delivered;
        return order;
    }

    /// <summary>Signal to cancel the order mid-flight.</summary>
    [WorkflowSignal]
    public Task CancelAsync(string reason)
    {
        _cancellationReason = reason;
        return Task.CompletedTask;
    }

    /// <summary>Query current status without modifying state.</summary>
    [WorkflowQuery]
    public OrderStatus GetStatus() => OrderStatus.Pending; // simplified
}
