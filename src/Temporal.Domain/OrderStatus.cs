namespace Temporal.Domain;

public enum OrderStatus
{
    Pending,
    PaymentProcessing,
    PaymentApproved,
    PaymentFailed,
    Picking,
    Shipping,
    Delivered,
    Cancelled
}
