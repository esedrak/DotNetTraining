using Bank.Domain;
using Bank.Domain.Exceptions;
using Bank.Repository;
using Microsoft.Extensions.Logging;

namespace Bank.Service;

public class BankService(IBankRepository repository, ILogger<BankService> logger) : IBankService
{
    public async Task<Account> CreateAccountAsync(
        string owner,
        decimal initialBalance = 0m,
        CancellationToken ct = default)
    {
        var account = new Account(owner, initialBalance);
        var created = await repository.CreateAccountAsync(account, ct);
        logger.LogInformation("Created account {AccountId} for owner {Owner}", created.Id, owner);
        return created;
    }

    public async Task<Account> GetAccountAsync(Guid id, CancellationToken ct = default)
    {
        var account = await repository.GetAccountAsync(id, ct);
        if (account is null)
        {
            logger.LogWarning("Account {AccountId} not found", id);
            throw new AccountNotFoundException(id);
        }
        return account;
    }

    public Task<IReadOnlyList<Account>> ListAccountsAsync(CancellationToken ct = default)
        => repository.ListAccountsAsync(ct);

    public async Task<Transfer> CreateTransferAsync(
        Guid fromId,
        Guid toId,
        decimal amount,
        CancellationToken ct = default)
    {
        // Fetch both accounts — throws AccountNotFoundException if either is missing
        var from = await GetAccountAsync(fromId, ct);
        var to = await GetAccountAsync(toId, ct);

        var transfer = new Transfer(from.Id, to.Id, amount);

        try
        {
            // Apply debit and credit
            from.Withdraw(amount);   // throws InsufficientFundsException if balance too low
            to.Deposit(amount);

            transfer.Complete();
        }
        catch (InsufficientFundsException ex)
        {
            transfer.Fail(ex.Message);
            logger.LogWarning("Transfer {TransferId} failed: {Reason}", transfer.Id, ex.Message);
            throw;
        }

        var saved = await repository.CreateTransferAsync(transfer, ct);
        logger.LogInformation("Transfer {TransferId} completed: {Amount} from {From} to {To}",
            saved.Id, amount, fromId, toId);
        return saved;
    }

    public async Task<Transfer> GetTransferAsync(Guid id, CancellationToken ct = default)
    {
        var transfer = await repository.GetTransferAsync(id, ct);
        if (transfer is null)
        {
            throw new KeyNotFoundException($"Transfer '{id}' not found.");
        }

        return transfer;
    }

    public Task<IReadOnlyList<Transfer>> ListTransfersAsync(CancellationToken ct = default)
        => repository.ListTransfersAsync(ct);
}
