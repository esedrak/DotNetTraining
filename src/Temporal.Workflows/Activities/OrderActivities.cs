using Microsoft.Extensions.Logging;
using Temporal.Domain;
using Temporalio.Activities;

namespace Temporal.Workflows.Activities;

/// <summary>
/// Order processing activities.
/// Activities are the units of work that actually DO things (call APIs, write to DB, etc.)
/// They can fail and will be retried according to the RetryPolicy.
/// </summary>
public class OrderActivities(ILogger<OrderActivities> logger)
{
    [Activity]
    public async Task<Order> ValidateOrderAsync(Order order)
    {
        ActivityExecutionContext.Current.CancellationToken.ThrowIfCancellationRequested();
        logger.LogInformation("Validating order {OrderId}", order.Id);

        if (order.Quantity <= 0)
        {
            throw new InvalidOperationException("Order quantity must be positive.");
        }

        if (order.TotalAmount <= 0)
        {
            throw new InvalidOperationException("Order total must be positive.");
        }

        await Task.Delay(100); // simulate validation
        return order;
    }

    [Activity]
    public async Task<Order> ProcessPaymentAsync(Order order)
    {
        ActivityExecutionContext.Current.CancellationToken.ThrowIfCancellationRequested();
        logger.LogInformation("Processing payment for order {OrderId}, amount {Amount}",
            order.Id, order.TotalAmount);

        await Task.Delay(200); // simulate payment gateway call
        return order;
    }

    [Activity]
    public async Task<Order> PickOrderAsync(Order order)
    {
        ActivityExecutionContext.Current.CancellationToken.ThrowIfCancellationRequested();
        logger.LogInformation("Picking order {OrderId}", order.Id);

        order.Status = OrderStatus.Picking;
        await Task.Delay(300); // simulate warehouse picking
        return order;
    }

    [Activity]
    public async Task<Order> ShipOrderAsync(Order order)
    {
        ActivityExecutionContext.Current.CancellationToken.ThrowIfCancellationRequested();
        logger.LogInformation("Shipping order {OrderId}", order.Id);

        order.Status = OrderStatus.Shipping;
        await Task.Delay(100); // simulate shipping label creation
        return order;
    }
}
