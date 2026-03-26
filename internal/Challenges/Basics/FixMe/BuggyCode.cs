namespace DotNetTraining.Challenges.Basics.FixMe;

// ── Challenge 1: Race condition ───────────────────────────────────────────────
// BUG: Multiple tasks increment _count concurrently without synchronization.
// Fix: Use thread-safe operations (Interlocked, lock, or SemaphoreSlim).

public class RaceCounter
{
    private int _count;

    public async Task IncrementManyAsync(int times)
    {
        var tasks = Enumerable.Range(0, times).Select(_ => Task.Run(() =>
        {
            _count++; // BUG: not thread-safe — data race!
        }));
        await Task.WhenAll(tasks);
    }

    public int Value => _count;
}

// ── Challenge 2: Async deadlock ────────────────────────────────────────────────
// BUG: Calling .Result on an async method blocks the current thread,
//      which can deadlock in contexts with a single-threaded synchronization context.
// Fix: Use async/await all the way up, or use ConfigureAwait(false).

public class DeadlockExample
{
    public string GetData()
    {
        return FetchDataAsync().Result; // BUG: blocking on async — potential deadlock!
    }

    private async Task<string> FetchDataAsync()
    {
        await Task.Delay(10);
        return "data";
    }
}

// ── Challenge 3: Null reference exception ────────────────────────────────────
// BUG: GetFirstName doesn't handle the case where names is null or empty.
// Fix: Add null and empty checks, or use null-conditional operators.

public class NullBug
{
    public string GetFirstName(List<string>? names)
    {
        return names[0].ToUpper(); // BUG: throws if names is null or empty!
    }
}

// ── Challenge 4: Off-by-one ───────────────────────────────────────────────────
// BUG: The last element is always skipped.
// Fix: Use correct loop bounds or LINQ.

public class OffByOne
{
    public int[] DoubleAll(int[] values)
    {
        var result = new int[values.Length];
        for (int i = 0; i < values.Length - 1; i++) // BUG: should be <= or just Length
            result[i] = values[i] * 2;
        return result;
    }
}

// ── Challenge 5: Improper resource disposal ───────────────────────────────────
// BUG: The HttpClient is created but never disposed, leaking socket connections.
// Fix: Use `using`, `IHttpClientFactory`, or a shared static HttpClient.

public class ResourceLeak
{
    public async Task<string> FetchAsync(string url)
    {
        var client = new HttpClient(); // BUG: not disposed, socket leak!
        return await client.GetStringAsync(url);
    }
}
