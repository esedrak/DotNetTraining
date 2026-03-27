namespace DotNetTraining.Basics.Interface;

// ── Basic interface definition and implementation ─────────────────────────────

public interface IGreeter
{
    string Greet(string name);
}

public class FormalGreeter : IGreeter
{
    public string Greet(string name) => $"Good day, {name}.";
}

public class CasualGreeter : IGreeter
{
    public string Greet(string name) => $"Hey {name}!";
}

// ── Consumer programmed to the interface ─────────────────────────────────────

/// <summary>
/// WelcomeService depends on IGreeter, not a concrete class.
/// The concrete implementation is injected (DI or test double).
/// </summary>
public class WelcomeService(IGreeter greeter)
{
    public string Welcome(string name) => greeter.Greet(name);
}

// ── Interface inheritance ────────────────────────────────────────────────────

public interface IReader<T>
{
    T? GetById(int id);
    IReadOnlyList<T> GetAll();
}

public interface IWriter<T>
{
    void Add(T entity);
    void Remove(int id);
}

/// <summary>Compose reader + writer into a full repository interface.</summary>
public interface IRepository<T> : IReader<T>, IWriter<T> { }

// ── In-memory implementation (useful for tests) ──────────────────────────────

public class InMemoryRepository<T> : IRepository<T>
{
    private readonly Dictionary<int, T> _store = [];
    private int _nextId = 1;

    public T? GetById(int id) => _store.TryGetValue(id, out var v) ? v : default;
    public IReadOnlyList<T> GetAll() => [.. _store.Values];
    public void Add(T entity) => _store[_nextId++] = entity;
    public void Remove(int id) => _store.Remove(id);
}

// ── Default interface methods (C# 8+) ────────────────────────────────────────

public interface ILogger
{
    void Log(string message);

    /// <summary>Default implementation — concrete types don't need to override.</summary>
    void LogError(string message) => Log($"[ERROR] {message}");
    void LogInfo(string message) => Log($"[INFO]  {message}");
}

public class ConsoleLogger : ILogger
{
    public void Log(string message) => Console.WriteLine(message);
    // LogError and LogInfo are inherited from the default interface implementation
}

// ── IDisposable — resource cleanup contract ───────────────────────────────────

public class ManagedResource : IDisposable
{
    private bool _disposed;

    public void DoWork()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        // ... do something
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        // Release unmanaged resources here
        GC.SuppressFinalize(this);
    }
}
