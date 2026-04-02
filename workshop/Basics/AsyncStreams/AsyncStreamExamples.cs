using System.Runtime.CompilerServices;

namespace DotNetTraining.Basics.AsyncStreams;

// ── Generator and combinators ─────────────────────────────────────────────────

public static class AsyncStreamExamples
{
    /// <summary>
    /// Produce a sequence of integers asynchronously, one at a time.
    /// The <see cref="EnumeratorCancellationAttribute"/> wires up the token
    /// supplied via <c>.WithCancellation(ct)</c> on the consumer side.
    /// </summary>
    public static async IAsyncEnumerable<int> GenerateAsync(
        int count,
        int delayMs = 0,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        for (int i = 0; i < count; i++)
        {
            ct.ThrowIfCancellationRequested();

            if (delayMs > 0)
            {
                await Task.Delay(delayMs, ct);
            }

            yield return i;
        }
    }

    /// <summary>
    /// Consume any async sequence and materialise it into a <see cref="List{T}"/>.
    /// Passes the cancellation token via <c>WithCancellation</c>.
    /// </summary>
    public static async Task<List<T>> ToListAsync<T>(
        IAsyncEnumerable<T> source,
        CancellationToken ct = default)
    {
        var result = new List<T>();

        await foreach (var item in source.WithCancellation(ct))
        {
            result.Add(item);
        }

        return result;
    }

    /// <summary>
    /// Project each element of an async sequence using <paramref name="selector"/>.
    /// Equivalent to LINQ <c>Select</c> for <see cref="IAsyncEnumerable{T}"/>.
    /// </summary>
    public static async IAsyncEnumerable<TResult> SelectAsync<T, TResult>(
        IAsyncEnumerable<T> source,
        Func<T, TResult> selector,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var item in source.WithCancellation(ct))
        {
            yield return selector(item);
        }
    }

    /// <summary>
    /// Yield only the elements of an async sequence that satisfy <paramref name="predicate"/>.
    /// Equivalent to LINQ <c>Where</c> for <see cref="IAsyncEnumerable{T}"/>.
    /// </summary>
    public static async IAsyncEnumerable<T> WhereAsync<T>(
        IAsyncEnumerable<T> source,
        Func<T, bool> predicate,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var item in source.WithCancellation(ct))
        {
            if (predicate(item))
            {
                yield return item;
            }
        }
    }

    /// <summary>
    /// Group items into non-overlapping chunks of at most <paramref name="size"/> elements.
    /// The final batch may be smaller than <paramref name="size"/>.
    /// </summary>
    public static async IAsyncEnumerable<IReadOnlyList<T>> BatchAsync<T>(
        IAsyncEnumerable<T> source,
        int size,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var batch = new List<T>(size);

        await foreach (var item in source.WithCancellation(ct))
        {
            batch.Add(item);

            if (batch.Count == size)
            {
                yield return batch.AsReadOnly();
                batch = new List<T>(size);
            }
        }

        if (batch.Count > 0)
        {
            yield return batch.AsReadOnly();
        }
    }
}

// ── Streaming I/O ─────────────────────────────────────────────────────────────

public static class FileLineReader
{
    /// <summary>
    /// Read lines from a <see cref="TextReader"/> one at a time without buffering the
    /// entire content in memory. Simulates streaming I/O — e.g. reading a large log file
    /// or a network response body delivered incrementally.
    /// </summary>
    public static async IAsyncEnumerable<string> ReadLinesAsync(
        TextReader reader,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        string? line;

        while ((line = await reader.ReadLineAsync(ct)) is not null)
        {
            yield return line;
        }
    }
}
