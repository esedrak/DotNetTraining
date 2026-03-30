// Temporal Client — starts an order workflow
//
// Usage: dotnet run --project src/Temporal.Client
//
// Prerequisites: make infra-up (starts Temporal server at localhost:7233)

using Temporal.Domain;
using Temporal.Workflows.Workflows;
using Temporalio.Client;

Console.WriteLine("Connecting to Temporal at localhost:7233...");

var client = await TemporalClient.ConnectAsync(new("localhost:7233"));

var order = new Order
{
    CustomerId = "customer-001",
    ProductId = "product-42",
    Quantity = 2,
    TotalAmount = 199.99m
};

Console.WriteLine($"Starting OrderWorkflow for order {order.Id}...");

var handle = await client.StartWorkflowAsync(
    (OrderWorkflow w) => w.RunAsync(order),
    new WorkflowOptions
    {
        Id = $"order-{order.Id}",
        TaskQueue = "order-processing"
    });

Console.WriteLine($"Workflow started: {handle.Id}");
Console.WriteLine("Waiting for result...");

var result = await handle.GetResultAsync();

Console.WriteLine($"Order completed: {result.Id}");
Console.WriteLine($"Status: {result.Status}");

