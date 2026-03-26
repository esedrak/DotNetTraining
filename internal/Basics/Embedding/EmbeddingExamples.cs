using System.Reflection;
using System.Text.Json;

namespace DotNetTraining.Basics.Embedding;

// ── Inheritance — C#'s equivalent of Go struct embedding for IS-A ─────────────

/// <summary>
/// Base entity with common fields. Inheritance gives derived types all base members.
/// Equivalent to Go: type TimestampedEntity struct { CreatedAt, UpdatedAt time.Time }
/// type Account struct { TimestampedEntity; ... }
/// </summary>
public abstract class TimestampedEntity
{
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}

public class BankAccount : TimestampedEntity
{
    public required Guid Id { get; init; }
    public required string Owner { get; init; }
    public decimal Balance { get; private set; }

    public void Deposit(decimal amount) { Balance += amount; Touch(); }
    public void Withdraw(decimal amount) { Balance -= amount; Touch(); }
}

// ── Composition — preferred for HAS-A / decorator relationships ───────────────

/// <summary>
/// Repository interface — what we want to compose around.
/// </summary>
public interface IDataStore<T>
{
    Task<T?> GetAsync(Guid id, CancellationToken ct = default);
    Task SaveAsync(T entity, CancellationToken ct = default);
}

/// <summary>
/// In-memory implementation (for tests / demos).
/// </summary>
public class InMemoryDataStore<T> : IDataStore<T>
{
    private readonly Dictionary<Guid, T> _store = [];
    private readonly Func<T, Guid> _getId;

    public InMemoryDataStore(Func<T, Guid> getId) { _getId = getId; }

    public Task<T?> GetAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_store.GetValueOrDefault(id));

    public Task SaveAsync(T entity, CancellationToken ct = default)
    {
        _store[_getId(entity)] = entity;
        return Task.CompletedTask;
    }
}

/// <summary>
/// Decorator: wraps IDataStore<T> and adds logging.
/// Go equivalent: type LoggingRepo struct { inner Repository; logger *slog.Logger }
/// C# uses composition (field) rather than struct embedding.
/// </summary>
public class LoggingDataStore<T>(
    IDataStore<T> inner,
    Microsoft.Extensions.Logging.ILogger logger)
    : IDataStore<T>
{
    public async Task<T?> GetAsync(Guid id, CancellationToken ct = default)
    {
        logger.LogInformation("Getting {Type} {Id}", typeof(T).Name, id);
        var result = await inner.GetAsync(id, ct);
        if (result is null) logger.LogWarning("{Type} {Id} not found", typeof(T).Name, id);
        return result;
    }

    public async Task SaveAsync(T entity, CancellationToken ct = default)
    {
        logger.LogInformation("Saving {Type}", typeof(T).Name);
        await inner.SaveAsync(entity, ct);
    }
}

// ── Embedded resources — C# equivalent of Go's //go:embed ────────────────────

/// <summary>
/// Demonstrates reading files embedded in the assembly at compile time.
///
/// To embed a file, add to .csproj:
///   &lt;EmbeddedResource Include="Embedding/sample.json" /&gt;
///
/// Resource names use dots as separators and include the default namespace:
///   "Basics.Embedding.sample_json"  (note: hyphens → underscore in accessor)
/// </summary>
public static class EmbeddedResources
{
    /// <summary>
    /// Read an embedded file by its manifest resource name.
    /// Equivalent to Go: //go:embed data.json; var data []byte
    /// </summary>
    public static string? ReadEmbeddedText(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null) return null;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// List all embedded resource names in the assembly.
    /// Useful for discovering what's embedded during development.
    /// </summary>
    public static IEnumerable<string> ListEmbeddedResources()
        => Assembly.GetExecutingAssembly().GetManifestResourceNames();
}

// ── Interface composition — C#'s primary composition mechanism ────────────────

/// <summary>
/// Composing multiple interfaces is the idiomatic .NET pattern.
/// More explicit than Go's implicit embedding, but equally flexible.
/// </summary>
public interface IReadable<T> { Task<T?> ReadAsync(Guid id, CancellationToken ct = default); }
public interface IWritable<T> { Task WriteAsync(T item, CancellationToken ct = default); }

// Composed interface — equivalent to embedding io.Reader + io.Writer in Go
public interface IReadWritable<T> : IReadable<T>, IWritable<T> { }
