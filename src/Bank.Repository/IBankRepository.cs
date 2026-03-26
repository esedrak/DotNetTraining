using Bank.Domain;

namespace Bank.Repository;

/// <summary>
/// Repository interface — the contract between service layer and data access.
/// Defines exactly what persistence operations the service needs.
/// </summary>
public interface IBankRepository
{
    // Accounts
    Task<Account?> GetAccountAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Account>> ListAccountsAsync(CancellationToken ct = default);
    Task<Account> CreateAccountAsync(Account account, CancellationToken ct = default);

    // Transfers
    Task<Transfer?> GetTransferAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Transfer>> ListTransfersAsync(CancellationToken ct = default);
    Task<Transfer> CreateTransferAsync(Transfer transfer, CancellationToken ct = default);
    Task UpdateTransferAsync(Transfer transfer, CancellationToken ct = default);
}
