using System.Diagnostics.CodeAnalysis;

namespace Bank.Domain;

/// <summary>
/// Bank account entity.
/// Uses a class (not record) for EF Core compatibility and controlled mutation.
/// </summary>
public class Account
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Owner { get; init; }
    public decimal Balance { get; private set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    // EF Core parameterless constructor
    private Account() { Owner = ""; }

    [SetsRequiredMembers]
    public Account(string owner, decimal initialBalance = 0m)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(owner, nameof(owner));
        ArgumentOutOfRangeException.ThrowIfNegative(initialBalance, nameof(initialBalance));
        Owner = owner;
        Balance = initialBalance;
    }

    public void Deposit(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount, nameof(amount));
        Balance += amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Withdraw(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount, nameof(amount));
        if (amount > Balance)
            throw new InsufficientFundsException(Id, amount, Balance);
        Balance -= amount;
        UpdatedAt = DateTime.UtcNow;
    }
}
