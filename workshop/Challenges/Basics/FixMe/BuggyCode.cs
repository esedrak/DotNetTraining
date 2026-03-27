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
        return names![0].ToUpper(); // BUG: throws if names is null or empty!
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

// ── Challenge 6: Struct mutation (copy semantics) ────────────────────────────
// BUG: Modifying a struct through a method operates on a copy, not the original.
// The balance change is silently lost because structs have value semantics.
// Fix: Use a class instead of a struct, or use `ref` to pass by reference.

public struct AccountBalance
{
    public string Owner;
    public decimal Balance;

    public void Deposit(decimal amount)
    {
        Balance += amount; // This works on the struct itself...
    }
}

public class StructMutationBug
{
    private readonly List<AccountBalance> _accounts = [];

    public void AddAccount(string owner, decimal balance)
        => _accounts.Add(new AccountBalance { Owner = owner, Balance = balance });

    public void DepositToFirst(decimal amount)
    {
        if (_accounts.Count == 0) return;
        var account = _accounts[0]; // BUG: this copies the struct!
        account.Deposit(amount);    // Mutation is on the copy, not the list element
        // The original _accounts[0] is unchanged!
    }

    public decimal GetFirstBalance()
        => _accounts.Count > 0 ? _accounts[0].Balance : 0;
}

// ── Challenge 7: Disposable leak in a loop ───────────────────────────────────
// BUG: Each iteration creates a new disposable resource that is never disposed.
// In a real app with SqlConnection, this exhausts the connection pool.
// Fix: Wrap each resource creation in a `using` statement.

public class DisposableLeakBug
{
    public static readonly List<string> DisposalLog = [];

    public class LeakyResource : IDisposable
    {
        public string Name { get; }
        public bool IsDisposed { get; private set; }

        public LeakyResource(string name) => Name = name;

        public string Process() => $"Processed {Name}";

        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                DisposalLog.Add(Name);
            }
        }
    }

    public List<string> ProcessMany(string[] names)
    {
        var results = new List<string>();
        foreach (var name in names)
        {
            var resource = new LeakyResource(name); // BUG: never disposed!
            results.Add(resource.Process());
        }
        return results;
    }
}

// ── Challenge 8: async void swallows exceptions ─────────────────────────────
// BUG: async void methods fire-and-forget. Exceptions crash the process
// or are silently lost (no Task to observe them).
// Fix: Change to async Task so callers can await and observe exceptions.

public class AsyncVoidBug
{
    public List<string> Log { get; } = [];

    public async void ProcessItemAsync(string item) // BUG: async void!
    {
        await Task.Delay(1);
        if (item == "bad")
            throw new InvalidOperationException("Processing failed!"); // Unobservable!
        Log.Add(item);
    }
}
