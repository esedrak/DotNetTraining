namespace Temporal.Domain;

/// <summary>Order domain entity for the Temporal workflow example.</summary>
public class Order
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string CustomerId { get; init; }
    public required string ProductId { get; init; }
    public int Quantity { get; init; }
    public decimal TotalAmount { get; init; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public string? FailureReason { get; set; }
}
