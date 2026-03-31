namespace Bank.Temporal.Models;

public record TransferInput(
    Guid TransferId,
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    string? Reference = null
);
