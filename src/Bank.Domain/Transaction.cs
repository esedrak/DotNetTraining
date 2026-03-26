namespace Bank.Domain;

public enum TransactionType
{
    Deposit,
    Withdrawal,
    Transfer
}

public class Transaction
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid AccountId { get; init; }
    public decimal Amount { get; init; }
    public TransactionType Type { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public string? Description { get; init; }

    // EF Core
    private Transaction() { }

    public Transaction(Guid accountId, decimal amount, TransactionType type, string? description = null)
    {
        AccountId = accountId;
        Amount = amount;
        Type = type;
        Description = description;
    }
}
