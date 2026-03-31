using Bank.Domain;
using Bank.Repository;
using Bank.Temporal.Models;
using Microsoft.Extensions.Logging;
using Temporalio.Activities;
using Temporalio.Exceptions;

namespace Bank.Temporal.Activities;

public class TransferActivities(IBankRepository repository, ILogger<TransferActivities> logger)
{
    private readonly IBankRepository _repository = repository;
    private readonly ILogger<TransferActivities> _logger = logger;

    [Activity]
    public async Task ValidateAccountsAsync(ValidateAccountsInput input)
    {
        if (input.Amount <= 0)
        {
            throw new ApplicationFailureException(
                $"Transfer amount must be positive, got {input.Amount}.", nonRetryable: true);
        }

        if (input.FromAccountId == input.ToAccountId)
        {
            throw new ApplicationFailureException(
                "Source and destination accounts must be different.", nonRetryable: true);
        }

        var from = await _repository.GetAccountAsync(input.FromAccountId);
        if (from is null)
        {
            throw new ApplicationFailureException(
                $"Source account '{input.FromAccountId}' not found.", nonRetryable: true);
        }

        var to = await _repository.GetAccountAsync(input.ToAccountId);
        if (to is null)
        {
            throw new ApplicationFailureException(
                $"Destination account '{input.ToAccountId}' not found.", nonRetryable: true);
        }
    }

    [Activity]
    public async Task DebitAccountAsync(DebitInput input)
    {
        var idempotencyKey = $"debit:{input.TransferId}";
        if (await _repository.TransactionExistsAsync(input.AccountId, idempotencyKey))
        {
            _logger.LogInformation("Debit already applied for transfer {TransferId}, skipping.", input.TransferId);
            return;
        }

        var account = await _repository.GetAccountAsync(input.AccountId);
        if (account is null)
        {
            throw new ApplicationFailureException(
                $"Account '{input.AccountId}' not found.", nonRetryable: true);
        }

        try
        {
            account.Withdraw(input.Amount);
        }
        catch (InsufficientFundsException ex)
        {
            throw new ApplicationFailureException(ex.Message, nonRetryable: true);
        }

        await _repository.CreateTransactionAsync(new Transaction(
            input.AccountId, input.Amount, TransactionType.Withdrawal, idempotencyKey));
    }

    [Activity]
    public async Task CreditAccountAsync(CreditInput input)
    {
        var idempotencyKey = $"credit:{input.TransferId}";
        if (await _repository.TransactionExistsAsync(input.AccountId, idempotencyKey))
        {
            _logger.LogInformation("Credit already applied for transfer {TransferId}, skipping.", input.TransferId);
            return;
        }

        var account = await _repository.GetAccountAsync(input.AccountId);
        if (account is null)
        {
            throw new ApplicationFailureException(
                $"Account '{input.AccountId}' not found.", nonRetryable: true);
        }

        account.Deposit(input.Amount);

        await _repository.CreateTransactionAsync(new Transaction(
            input.AccountId, input.Amount, TransactionType.Deposit, idempotencyKey));
    }

    [Activity]
    public async Task RefundDebitAsync(RefundInput input)
    {
        var idempotencyKey = $"refund:{input.TransferId}";
        if (await _repository.TransactionExistsAsync(input.AccountId, idempotencyKey))
        {
            _logger.LogInformation("Refund already applied for transfer {TransferId}, skipping.", input.TransferId);
            return;
        }

        var account = await _repository.GetAccountAsync(input.AccountId);
        if (account is null)
        {
            throw new ApplicationFailureException(
                $"Account '{input.AccountId}' not found.", nonRetryable: true);
        }

        account.Deposit(input.Amount);

        await _repository.CreateTransactionAsync(new Transaction(
            input.AccountId, input.Amount, TransactionType.Deposit, idempotencyKey));
    }
}
