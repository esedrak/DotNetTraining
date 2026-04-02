namespace DotNetTraining.Basics.ErrorHandling;

// ── Custom exception types ────────────────────────────────────────────────────

/// <summary>Domain exception for a missing resource.</summary>
public class NotFoundException : Exception
{
    public string ResourceType { get; }
    public object Id { get; }

    public NotFoundException(string resourceType, object id)
        : base($"{resourceType} with id '{id}' was not found.")
    {
        ResourceType = resourceType;
        Id = id;
    }
}

/// <summary>Wrap an inner exception with context, preserving the original as InnerException.</summary>
public class OperationFailedException : Exception
{
    public OperationFailedException(string operation, Exception inner)
        : base($"Operation '{operation}' failed: {inner.Message}", inner)
    { }
}

// ── Exception throwing and catching ──────────────────────────────────────────

public class BankService
{
    private readonly Dictionary<int, string> _accounts = new() { [1] = "Alice" };

    /// <summary>Throws a specific exception type — callers catch by type.</summary>
    public string GetAccount(int id)
    {
        if (!_accounts.TryGetValue(id, out var name))
        {
            throw new NotFoundException("Account", id);
        }

        return name;
    }

    /// <summary>
    /// Exception filter with `when` clause.
    /// Only catches if the condition is true — useful for conditional retry/logging.
    /// </summary>
    public string GetAccountWithFilter(int id)
    {
        try
        {
            return GetAccount(id);
        }
        catch (NotFoundException ex) when (ex.Id is int accountId && accountId > 0)
        {
            return $"[Not Found: id={accountId}]";
        }
    }

    /// <summary>finally always runs — use it for cleanup (or prefer `using`).</summary>
    public void TransferWithCleanup(int fromId, int toId)
    {
        bool lockAcquired = false;
        try
        {
            lockAcquired = true;
            _ = GetAccount(fromId);
            _ = GetAccount(toId);
            // ... transfer logic
        }
        finally
        {
            if (lockAcquired)
            {
                // Release locks, connections, etc.
                lockAcquired = false;
                _ = lockAcquired;
            }
        }
    }
}

// ── Result<T> pattern ─────────────────────────────────────────────────────────

/// <summary>
/// Discriminated union for success/failure without exceptions.
/// Use for *expected* failure paths where the caller should handle both cases.
/// </summary>
public readonly record struct Result<T>
{
    private readonly T? _value;
    private readonly string? _error;

    private Result(T value) => _value = value;
    private Result(string error) => _error = error;

    public bool IsSuccess => _error is null;
    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("Result has no value.");
    public string Error => !IsSuccess ? _error! : throw new InvalidOperationException("Result has no error.");

    public static Result<T> Ok(T value) => new(value);
    public static Result<T> Fail(string error) => new(error);

    /// <summary>Transform the value if successful.</summary>
    public Result<TOut> Map<TOut>(Func<T, TOut> transform) =>
        IsSuccess ? Result<TOut>.Ok(transform(Value)) : Result<TOut>.Fail(Error);

    public override string ToString() =>
        IsSuccess ? $"Ok({Value})" : $"Fail({Error})";
}

public static class ResultExamples
{
    /// <summary>Parse a positive integer — returns Result instead of throwing.</summary>
    public static Result<int> ParsePositive(string input)
    {
        if (!int.TryParse(input, out int value))
        {
            return Result<int>.Fail($"'{input}' is not a valid integer.");
        }

        if (value <= 0)
        {
            return Result<int>.Fail($"Expected positive integer, got {value}.");
        }

        return Result<int>.Ok(value);
    }
}
