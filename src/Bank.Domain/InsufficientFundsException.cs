namespace Bank.Domain;

public class InsufficientFundsException : Exception
{
    public Guid AccountId { get; }
    public decimal Requested { get; }
    public decimal Available { get; }

    public InsufficientFundsException(Guid accountId, decimal requested, decimal available)
        : base($"Insufficient funds in account '{accountId}': requested {requested:C}, available {available:C}.")
    {
        AccountId = accountId;
        Requested = requested;
        Available = available;
    }
}
