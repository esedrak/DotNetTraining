using Bank.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bank.Repository;

public class PostgresBankRepository(BankDbContext db) : IBankRepository
{
    public async Task<Account?> GetAccountAsync(Guid id, CancellationToken ct = default)
        => await db.Accounts.FindAsync([id], ct);

    public async Task<IReadOnlyList<Account>> ListAccountsAsync(CancellationToken ct = default)
        => await db.Accounts.OrderBy(a => a.CreatedAt).ToListAsync(ct);

    public async Task<Account> CreateAccountAsync(Account account, CancellationToken ct = default)
    {
        db.Accounts.Add(account);
        await db.SaveChangesAsync(ct);
        return account;
    }

    public async Task<Transfer?> GetTransferAsync(Guid id, CancellationToken ct = default)
        => await db.Transfers.FindAsync([id], ct);

    public async Task<IReadOnlyList<Transfer>> ListTransfersAsync(CancellationToken ct = default)
        => await db.Transfers.OrderByDescending(t => t.CreatedAt).ToListAsync(ct);

    public async Task<Transfer> CreateTransferAsync(Transfer transfer, CancellationToken ct = default)
    {
        db.Transfers.Add(transfer);
        await db.SaveChangesAsync(ct);
        return transfer;
    }

    public async Task UpdateTransferAsync(Transfer transfer, CancellationToken ct = default)
    {
        db.Transfers.Update(transfer);
        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> TransactionExistsAsync(Guid accountId, string description, CancellationToken ct = default)
        => await db.Transactions.AnyAsync(
            t => t.AccountId == accountId && t.Description == description, ct);

    public async Task CreateTransactionAsync(Transaction transaction, CancellationToken ct = default)
    {
        db.Transactions.Add(transaction);
        await db.SaveChangesAsync(ct);
    }
}
