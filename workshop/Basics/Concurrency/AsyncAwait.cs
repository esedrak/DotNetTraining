using System.Threading.Channels;

namespace DotNetTraining.Basics.Concurrency;

// ── Task.Run — equivalent to `go func()` ────────────────────────────────────

public static class TaskExamples
{
    /// <summary>
    /// Run work concurrently and wait for all to finish.
    /// Equivalent to spawning goroutines + sync.WaitGroup.
    /// </summary>
    public static async Task<int[]> RunConcurrentAsync(int count)
    {
        var tasks = Enumerable.Range(0, count)
            .Select(i => Task.Run(() => i * i))
            .ToArray();

        return await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Return the first result — equivalent to Go's select on multiple channels.
    /// </summary>
    public static async Task<int> FirstToFinishAsync()
    {
        var slow = Task.Delay(500).ContinueWith(_ => 1);
        var fast = Task.Delay(100).ContinueWith(_ => 2);

        var winner = await Task.WhenAny(slow, fast);
        return await winner;
    }
}

// ── Channel<T> — equivalent to Go's chan T ────────────────────────────────────

public static class ChannelExamples
{
    /// <summary>
    /// Produce integers on a channel and consume them.
    /// Pattern: producer writes, consumer reads, writer.Complete() closes the channel.
    /// </summary>
    public static async Task<List<int>> ProduceAndConsumeAsync(int count)
    {
        var channel = Channel.CreateUnbounded<int>();
        var results = new List<int>();

        // Producer — equivalent to `go func() { ch <- i; close(ch) }()`
        _ = Task.Run(async () =>
        {
            for (int i = 0; i < count; i++)
            {
                await channel.Writer.WriteAsync(i);
            }

            channel.Writer.Complete();
        });

        // Consumer — equivalent to `for v := range ch`
        await foreach (var item in channel.Reader.ReadAllAsync())
        {
            results.Add(item);
        }

        return results;
    }

    /// <summary>
    /// Bounded channel — blocks the writer when full (backpressure).
    /// Use Channel.CreateBounded(capacity) for flow control.
    /// </summary>
    public static Channel<T> CreateBoundedChannel<T>(int capacity) =>
        Channel.CreateBounded<T>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        });
}

// ── Synchronization primitives ───────────────────────────────────────────────

public class SafeCounter
{
    private readonly object _lock = new();
    private int _count;

    /// <summary>Thread-safe increment using `lock` (equivalent to sync.Mutex).</summary>
    public void Increment()
    {
        lock (_lock)
        {
            _count++;
        }
    }

    public int Value
    {
        get
        {
            lock (_lock)
            {
                return _count;
            }
        }
    }
}

public class AsyncSafeCounter
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private int _count;

    /// <summary>
    /// Async-compatible mutual exclusion via SemaphoreSlim.
    /// Use this when the protected section contains awaits (lock doesn't allow await inside).
    /// </summary>
    public async Task IncrementAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            _count++;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public int Value => _count;
}

/// <summary>
/// Interlocked provides lock-free atomic operations.
/// Equivalent to Go's sync/atomic package.
/// </summary>
public static class AtomicCounter
{
    private static int _value;

    public static void Increment() => Interlocked.Increment(ref _value);
    public static int Read() => Interlocked.CompareExchange(ref _value, 0, 0);
}

/// <summary>
/// Lazy&lt;T&gt; is the equivalent of sync.Once — thread-safe single initialization.
/// </summary>
public class SingletonService
{
    private static readonly Lazy<SingletonService> _instance =
        new(() => new SingletonService(), isThreadSafe: true);

    private SingletonService() { }

    public static SingletonService Instance => _instance.Value;
    public string Name => "SingletonService";
}
