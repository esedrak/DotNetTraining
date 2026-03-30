using Temporal.Domain;
using Temporal.Workflows.Activities;
using Temporalio.Workflows;

namespace Temporal.Workflows.Workflows;

/// <summary>
/// Payment processing child workflow.
/// </summary>
[Workflow]
public class PaymentWorkflow
{
    [WorkflowRun]
    public async Task<Order> RunAsync(Order order)
    {
        order.Status = OrderStatus.PaymentProcessing;

        try
        {
            order = await Workflow.ExecuteActivityAsync(
                (OrderActivities a) => a.ProcessPaymentAsync(order),
                new ActivityOptions
                {
                    StartToCloseTimeout = TimeSpan.FromMinutes(1),
                    RetryPolicy = new()
                    {
                        MaximumAttempts = 3,
                        InitialInterval = TimeSpan.FromSeconds(1),
                        BackoffCoefficient = 2.0f
                    }
                });

            order.Status = OrderStatus.PaymentApproved;
        }
        catch (Exception)
        {
            order.Status = OrderStatus.PaymentFailed;
        }

        return order;
    }
}
