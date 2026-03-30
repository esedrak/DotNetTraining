namespace DotNetTraining.Basics.Context;

// ── Basic cancellation patterns ───────────────────────────────────────────────

public static class CancellationExamples
{
    /// <summary>
    /// Simulate a long-running operation that respects cancellation.
    /// Check token.IsCancellationRequested (or token.ThrowIfCancellationRequested)
    /// at safe points throughout the loop.
    /// </summary>
    public static async Task<int> CountToAsync(int limit, CancellationToken cancellationToken = default)
    {
        int count = 0;
        for (int i = 0; i < limit; i++)
        {
            // Cooperative cancellation check
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(10, cancellationToken);
            count++;
        }
        return count;
    }

    /// <summary>
    /// Demonstrates cancellation with a timeout.
    /// Equivalent to: ctx, cancel := context.WithTimeout(ctx, 100*time.Millisecond)
    /// </summary>
    public static async Task<bool> TryWorkWithTimeoutAsync(int workMs)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
        try
        {
            await Task.Delay(workMs, cts.Token);
            return true;  // completed within timeout
        }
        catch (OperationCanceledException)
        {
            return false; // timed out
        }
    }

    /// <summary>
    /// Demonstrates linking a parent token with a local deadline.
    /// If the parent cancels OR the deadline expires, work stops.
    /// Equivalent to context.WithTimeout on a parent context.
    /// </summary>
    public static async Task ProcessWithLinkedTokenAsync(
        CancellationToken parentToken,
        TimeSpan localTimeout)
    {
        using var localCts = new CancellationTokenSource(localTimeout);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(parentToken, localCts.Token);

        await Task.Delay(10, linked.Token); // will cancel if either source fires
    }
}

// ── AsyncLocal<T> — ambient values (like context.WithValue) ──────────────────

public static class AsyncLocalExamples
{
    /// <summary>
    /// AsyncLocal&lt;T&gt; flows a value through an async call chain without passing it explicitly.
    /// Use sparingly — prefer explicit parameters for most cases.
    /// </summary>
    private static readonly AsyncLocal<string?> _correlationId = new();

    public static string? CorrelationId
    {
        get => _correlationId.Value;
        set => _correlationId.Value = value;
    }

    public static async Task SimulateRequestAsync(string requestId)
    {
        CorrelationId = requestId;
        await Task.Yield(); // simulate async hop
        // CorrelationId is still available on the continuation
        _ = CorrelationId; // still == requestId
    }
}
