using Bank.Domain;

namespace Bank.Service;

public interface IBankService
{
    Task<Account> CreateAccountAsync(string owner, decimal initialBalance = 0m, CancellationToken ct = default);
    Task<Account> GetAccountAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Account>> ListAccountsAsync(CancellationToken ct = default);

    Task<Transfer> CreateTransferAsync(Guid fromId, Guid toId, decimal amount, CancellationToken ct = default);
    Task<Transfer> GetTransferAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Transfer>> ListTransfersAsync(CancellationToken ct = default);
}
