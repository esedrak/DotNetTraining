namespace DotNetTraining.Challenges.Basics.FixMe;

// ── Challenge 1: Race condition ───────────────────────────────────────────────
// FIX: Use Interlocked.Increment for atomic, thread-safe increments.

public class RaceCounter
{
    private int _count;

    public async Task IncrementManyAsync(int times)
    {
        var tasks = Enumerable.Range(0, times).Select(_ => Task.Run(() =>
        {
            Interlocked.Increment(ref _count); // FIX: atomic operation, no data race
        }));
        await Task.WhenAll(tasks);
    }

    public int Value => _count;
}

// ── Challenge 2: Async deadlock ────────────────────────────────────────────────
// FIX: Propagate async/await all the way up instead of blocking with .Result.

public class DeadlockExample
{
    public async Task<string> GetDataAsync() // FIX: async Task<string>, not blocking string
    {
        return await FetchDataAsync();
    }

    private async Task<string> FetchDataAsync()
    {
        await Task.Delay(10);
        return "data";
    }
}

// ── Challenge 3: Null reference exception ────────────────────────────────────
// FIX: Guard against null and empty input before indexing.

public class NullBug
{
    public string? GetFirstName(List<string>? names)
    {
        if (names is null || names.Count == 0)
        {
            return null;
        }

        return names[0].ToUpper();
    }
}

// ── Challenge 4: Off-by-one ───────────────────────────────────────────────────
// FIX: Loop to values.Length (not Length - 1) so the last element is included.

public class OffByOne
{
    public int[] DoubleAll(int[] values)
    {
        var result = new int[values.Length];
        for (int i = 0; i < values.Length; i++) // FIX: removed erroneous - 1
        {
            result[i] = values[i] * 2;
        }

        return result;
    }
}

// ── Challenge 5: Improper resource disposal ───────────────────────────────────
// FIX: Wrap HttpClient in a using statement so it is disposed after the request.

public class ResourceLeak
{
    public async Task<string> FetchAsync(string url)
    {
        using var client = new HttpClient(); // FIX: disposed when request completes
        return await client.GetStringAsync(url);
    }
}

// ── Challenge 6: Struct mutation (copy semantics) ────────────────────────────
// FIX: Change AccountBalance from a struct to a class so the list holds a
//      reference and mutations via Deposit are visible through the original reference.

public class AccountBalance // FIX: class, not struct — reference semantics
{
    public string Owner = "";
    public decimal Balance;

    public void Deposit(decimal amount)
    {
        Balance += amount;
    }
}

public class StructMutationBug
{
    private readonly List<AccountBalance> _accounts = [];

    public void AddAccount(string owner, decimal balance)
        => _accounts.Add(new AccountBalance { Owner = owner, Balance = balance });

    public void DepositToFirst(decimal amount)
    {
        if (_accounts.Count == 0)
        {
            return;
        }

        var account = _accounts[0]; // FIX: now a reference — mutation is visible
        account.Deposit(amount);
    }

    public decimal GetFirstBalance()
        => _accounts.Count > 0 ? _accounts[0].Balance : 0;
}

// ── Challenge 7: Disposable leak in a loop ───────────────────────────────────
// FIX: Add `using` so each resource is disposed at the end of its loop iteration.

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
            using var resource = new LeakyResource(name); // FIX: disposed after each iteration
            results.Add(resource.Process());
        }
        return results;
    }
}

// ── Challenge 8: async void swallows exceptions ─────────────────────────────
// FIX: Return async Task so callers can await the method and observe exceptions.

public class AsyncVoidBug
{
    public List<string> Log { get; } = [];

    public async Task ProcessItemAsync(string item) // FIX: async Task, not async void
    {
        await Task.Delay(1);
        if (item == "bad")
        {
            throw new InvalidOperationException("Processing failed!");
        }

        Log.Add(item);
    }
}
