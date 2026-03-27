namespace DotNetTraining.Basics.Linq;

// ── LINQ — Language Integrated Query ─────────────────────────────────────────
//
// LINQ provides a unified query syntax for collections, databases, XML, and more.
// Two syntaxes: method syntax (fluent) and query syntax (SQL-like).
// Key concept: deferred execution — queries are not evaluated until iterated.

/// <summary>A simple bank transaction for LINQ demonstrations.</summary>
public record Transaction(string Owner, string Description, decimal Amount, DateTimeOffset Date);

/// <summary>A bank account with an owner and balance, for LINQ grouping/filtering.</summary>
public record AccountSummary(string Owner, decimal Balance);

public static class LinqExamples
{
    /// <summary>
    /// Filter accounts that are overdrawn (negative balance).
    /// Demonstrates: Where (filtering).
    /// </summary>
    public static IEnumerable<AccountSummary> GetOverdrawnAccounts(IEnumerable<AccountSummary> accounts)
        => accounts.Where(a => a.Balance < 0);

    /// <summary>
    /// Get all distinct owners from a list of transactions.
    /// Demonstrates: Select (projection) + Distinct.
    /// </summary>
    public static IEnumerable<string> GetDistinctOwners(IEnumerable<Transaction> transactions)
        => transactions.Select(t => t.Owner).Distinct();

    /// <summary>
    /// Calculate the total balance across all accounts.
    /// Demonstrates: Sum (aggregation).
    /// </summary>
    public static decimal TotalBalance(IEnumerable<AccountSummary> accounts)
        => accounts.Sum(a => a.Balance);

    /// <summary>
    /// Group transactions by owner and sum their amounts.
    /// Demonstrates: GroupBy + Select with aggregation.
    /// </summary>
    public static IEnumerable<AccountSummary> SumByOwner(IEnumerable<Transaction> transactions)
        => transactions
            .GroupBy(t => t.Owner)
            .Select(g => new AccountSummary(g.Key, g.Sum(t => t.Amount)));

    /// <summary>
    /// Find the single largest transaction by absolute amount.
    /// Demonstrates: MaxBy.
    /// </summary>
    public static Transaction? LargestTransaction(IEnumerable<Transaction> transactions)
        => transactions.MaxBy(t => Math.Abs(t.Amount));

    /// <summary>
    /// Return the top N transactions ordered by amount descending.
    /// Demonstrates: OrderByDescending + Take.
    /// </summary>
    public static IEnumerable<Transaction> TopTransactions(IEnumerable<Transaction> transactions, int count)
        => transactions.OrderByDescending(t => t.Amount).Take(count);

    /// <summary>
    /// Check if any account is overdrawn.
    /// Demonstrates: Any (short-circuit evaluation).
    /// </summary>
    public static bool AnyOverdrawn(IEnumerable<AccountSummary> accounts)
        => accounts.Any(a => a.Balance < 0);

    // ── Query syntax examples ────────────────────────────────────────────────

    /// <summary>
    /// Same as SumByOwner but using query syntax instead of method syntax.
    /// Demonstrates: from/where/group/select (SQL-like) syntax.
    /// </summary>
    public static IEnumerable<AccountSummary> SumByOwnerQuery(IEnumerable<Transaction> transactions)
        => from t in transactions
           group t by t.Owner into g
           select new AccountSummary(g.Key, g.Sum(t => t.Amount));

    // ── Deferred execution demonstration ─────────────────────────────────────

    /// <summary>
    /// Returns a query that filters transactions — NOT yet evaluated.
    /// The filter runs only when the result is enumerated (foreach, ToList, etc.).
    /// </summary>
    public static IEnumerable<Transaction> DeferredFilter(
        IEnumerable<Transaction> transactions,
        decimal minAmount)
        => transactions.Where(t => t.Amount >= minAmount);
}
