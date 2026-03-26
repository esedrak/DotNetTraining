namespace Bank.Domain;

public enum TransferStatus
{
    Pending,
    Completed,
    Failed
}

public class Transfer
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid FromAccountId { get; init; }
    public Guid ToAccountId { get; init; }
    public decimal Amount { get; init; }
    public TransferStatus Status { get; private set; } = TransferStatus.Pending;
    public string? FailureReason { get; private set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    // EF Core
    private Transfer() { }

    public Transfer(Guid fromAccountId, Guid toAccountId, decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount, nameof(amount));
        if (fromAccountId == toAccountId)
            throw new ArgumentException("Cannot transfer to the same account.");
        FromAccountId = fromAccountId;
        ToAccountId = toAccountId;
        Amount = amount;
    }

    public void Complete()
    {
        Status = TransferStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Fail(string reason)
    {
        Status = TransferStatus.Failed;
        FailureReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }
}
