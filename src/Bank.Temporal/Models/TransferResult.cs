using Bank.Domain;

namespace Bank.Temporal.Models;

public record TransferResult(
    Guid TransferId,
    TransferStatus Status,
    string? FailureReason = null
);
