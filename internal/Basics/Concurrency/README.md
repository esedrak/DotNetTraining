# ⚡ Concurrency in C#

C# uses **async/await**, **`Task`**, and **`Channel<T>`** where Go uses goroutines and channels. The mental model is similar — concurrent units of work communicate through typed channels or await results — but the primitives differ.

---

## 1. Core Concepts

| Concept | Description |
| :--- | :--- |
| **`Task` / `Task<T>`** | Represents an asynchronous operation (like a goroutine's future) |
| **`async` / `await`** | Syntax for non-blocking asynchronous code |
| **`Task.Run()`** | Schedule work on the thread pool (like `go func()`) |
| **`Task.WhenAll()`** | Wait for all tasks (like `sync.WaitGroup`) |
| **`Task.WhenAny()`** | Return first completed task (like Go's `select`) |
| **`Channel<T>`** | Typed, async-safe FIFO queue (like Go's `chan T`) |
| **`lock`** | Mutual exclusion (like `sync.Mutex`) |
| **`SemaphoreSlim`** | Rate-limiting / async-compatible mutex |
| **`Interlocked`** | Atomic operations (like `sync/atomic`) |
| **`Lazy<T>`** | Thread-safe once-initialization (like `sync.Once`) |

---

## 2. Visual: Goroutine vs Task

```mermaid
flowchart LR
    subgraph Go
        G1["go func()"] --> CH["chan T"]
        G2["go func()"] --> CH
        CH --> R["Receiver goroutine"]
    end
    subgraph CSharp ["C#"]
        T1["Task.Run(...)"] --> CH2["Channel&lt;T&gt;"]
        T2["Task.Run(...)"] --> CH2
        CH2 --> R2["await reader.ReadAsync()"]
    end
```

---

## 3. Go → C# Mapping

| Go | C# |
| :--- | :--- |
| `go func() { ... }()` | `Task.Run(() => ...)` |
| `chan T` | `Channel<T>` |
| `ch <- value` | `await writer.WriteAsync(value)` |
| `v := <-ch` | `var v = await reader.ReadAsync()` |
| `close(ch)` | `writer.Complete()` |
| `select { case v := <-ch: }` | `Task.WhenAny(...)` |
| `sync.WaitGroup` | `Task.WhenAll(tasks)` |
| `sync.Mutex` | `lock` / `SemaphoreSlim` |
| `sync.RWMutex` | `ReaderWriterLockSlim` |
| `sync.Once` | `Lazy<T>` |
| `atomic.AddInt64` | `Interlocked.Add` |

---

## 4. Implementation Examples

### Launching concurrent work (go func → Task.Run)

```csharp
// Fire and forget equivalent to: go func() { doWork() }()
_ = Task.Run(() => DoWork());

// With result — await the Task
var result = await Task.Run(() => ComputeSomething());
```

### WaitGroup equivalent — Task.WhenAll

```csharp
var tasks = Enumerable.Range(0, 5)
    .Select(i => Task.Run(() => Process(i)));

await Task.WhenAll(tasks);
Console.WriteLine("All done");
```

### Channels

```csharp
var channel = Channel.CreateUnbounded<int>();

// Producer
_ = Task.Run(async () => {
    for (int i = 0; i < 5; i++)
        await channel.Writer.WriteAsync(i);
    channel.Writer.Complete();
});

// Consumer
await foreach (var item in channel.Reader.ReadAllAsync())
    Console.WriteLine(item);
```

### Mutex (lock)

```csharp
private readonly object _lock = new();
private int _count;

public void SafeIncrement()
{
    lock (_lock)
        _count++;
}
```

---

## ⚠️ Pitfalls & Best Practices

> [!WARNING]
> Never use `async void` — exceptions are unobservable and can crash the process. Use `async Task` instead. The only valid use of `async void` is event handlers.

1. `await` does **not** create a new thread — it suspends the current method and returns control to the caller.
2. `Task.Run` schedules on the thread pool — use for CPU-bound work. For I/O use `async`/`await` directly.
3. Don't `task.Result` or `task.Wait()` — this blocks the thread and can cause deadlocks.
4. `Channel<T>` is the idiomatic producer-consumer primitive; avoid `ConcurrentQueue` for new code.

---

## 🏃 Running the Examples

```bash
dotnet test tests/Basics.Tests --filter "FullyQualifiedName~Concurrency"
```

---

## 📚 Further Reading

- [Asynchronous programming (C# docs)](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/)
- [System.Threading.Channels](https://learn.microsoft.com/en-us/dotnet/core/extensions/channels)
- [Task Parallel Library](https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/task-parallel-library-tpl)

## Your Next Step
Now that you're running multiple tasks concurrently, you need a way to manage their lifecycles, cancellations, and timeouts.
Explore **[Context & CancellationToken](../Context/README.md)** to learn how to propagate deadlines and signals across your application.
