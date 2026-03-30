namespace DotNetTraining.Challenges.Basics.ImplMe;

// ── Challenge 1: Async pipeline ───────────────────────────────────────────────

public static class AsyncPipeline
{
    /// <summary>
    /// Process items concurrently but limit to <paramref name="maxConcurrency"/> at once.
    /// Think: a semaphore that allows only N concurrent operations.
    /// </summary>
    public static async Task<IReadOnlyList<TResult>> ProcessConcurrentlyAsync<T, TResult>(
        IEnumerable<T> items,
        Func<T, CancellationToken, Task<TResult>> processor,
        int maxConcurrency,
        CancellationToken ct = default)
    {
        var semaphore = new SemaphoreSlim(maxConcurrency);
        var tasks = items.Select(async item =>
        {
            await semaphore.WaitAsync(ct);
            try
            {
                return await processor(item, ct);
            }
            finally
            {
                semaphore.Release();
            }
        });
        return await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Retry <paramref name="operation"/> up to <paramref name="maxAttempts"/> times,
    /// with exponential backoff starting at <paramref name="initialDelay"/>.
    /// Rethrows the last exception if all attempts fail.
    /// </summary>
    public static async Task<T> RetryAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        int maxAttempts,
        TimeSpan initialDelay,
        CancellationToken ct = default)
    {
        Exception? lastException = null;
        var delay = initialDelay;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            try
            {
                return await operation(ct);
            }
            catch (Exception ex)
            {
                lastException = ex;
                if (attempt < maxAttempts - 1)
                {
                    await Task.Delay(delay, ct);
                    delay *= 2;
                }
            }
        }

        throw lastException!;
    }
}

// ── Challenge 2: Custom middleware ────────────────────────────────────────────

/// <summary>
/// ASP.NET Core middleware that adds a request correlation ID header.
/// If the incoming request has an "X-Correlation-ID" header, use that value.
/// Otherwise, generate a new GUID and add it to both the request and response headers.
/// </summary>
public class CorrelationIdMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;
    public const string HeaderName = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[HeaderName].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        context.Request.Headers[HeaderName] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        await _next(context);
    }
}

// ── Challenge 3: LINQ banking queries ────────────────────────────────────────

public record BankTransaction(string Owner, decimal Amount, string Category);

public static class LinqChallenge
{
    /// <summary>
    /// Return all transactions where the amount is negative (withdrawals).
    /// Use LINQ Where.
    /// </summary>
    public static IEnumerable<BankTransaction> GetWithdrawals(IEnumerable<BankTransaction> transactions)
        => transactions.Where(t => t.Amount < 0);

    /// <summary>
    /// Calculate the total balance (sum of all amounts) grouped by owner.
    /// Return a dictionary of Owner → TotalBalance.
    /// Use LINQ GroupBy + ToDictionary.
    /// </summary>
    public static Dictionary<string, decimal> TotalByOwner(IEnumerable<BankTransaction> transactions)
        => transactions
            .GroupBy(t => t.Owner)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

    /// <summary>
    /// Find the single largest transaction by absolute amount.
    /// Return null if the collection is empty.
    /// Use LINQ MaxBy or OrderByDescending.
    /// </summary>
    public static BankTransaction? LargestByAbsoluteAmount(IEnumerable<BankTransaction> transactions)
        => transactions.MaxBy(t => Math.Abs(t.Amount));
}
