using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Temporal.Domain;
using Temporalio.Activities;

namespace Temporal.Workflows.Activities;

/// <summary>
/// Inventory check activities — call an external inventory service via HTTP.
/// In tests and local dev, WireMock stubs the responses (see <c>wiremock/</c>).
/// </summary>
public class InventoryActivities(HttpClient httpClient, ILogger<InventoryActivities> logger)
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    [Activity]
    public async Task<InventoryCheckResult> CheckInventoryAsync(Order order)
    {
        ActivityExecutionContext.Current.CancellationToken.ThrowIfCancellationRequested();

        logger.LogInformation("Checking inventory for order {OrderId} — {ProductId} x{Quantity}",
            order.Id, order.ProductId, order.Quantity);

        var response = await httpClient.GetAsync(
            $"/inventory/{order.ProductId}",
            ActivityExecutionContext.Current.CancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Inventory service returned {StatusCode} for product {ProductId}",
                response.StatusCode, order.ProductId);
            return new InventoryCheckResult(order.ProductId, Available: false, StockLevel: 0);
        }

        var stream = await response.Content.ReadAsStreamAsync(
            ActivityExecutionContext.Current.CancellationToken);
        var result = await JsonSerializer.DeserializeAsync<InventoryCheckResult>(
            stream, JsonOptions, ActivityExecutionContext.Current.CancellationToken);

        return result ?? new InventoryCheckResult(order.ProductId, Available: false, StockLevel: 0);
    }
}

/// <summary>Response shape returned by the inventory service.</summary>
public record InventoryCheckResult(
    [property: JsonPropertyName("productId")] string ProductId,
    [property: JsonPropertyName("available")] bool Available,
    [property: JsonPropertyName("stockLevel")] int StockLevel);
