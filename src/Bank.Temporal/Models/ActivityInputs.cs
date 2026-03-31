namespace Bank.Temporal.Models;

public record ValidateAccountsInput(Guid FromAccountId, Guid ToAccountId, decimal Amount);
public record DebitInput(Guid AccountId, decimal Amount, Guid TransferId);
public record CreditInput(Guid AccountId, decimal Amount, Guid TransferId);
public record RefundInput(Guid AccountId, decimal Amount, Guid TransferId);
