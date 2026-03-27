namespace DotNetTraining.Basics.Disposable;

// ── IDisposable & Resource Management ────────────────────────────────────────
//
// Types that hold unmanaged resources (file handles, DB connections, sockets)
// implement IDisposable. The `using` statement/declaration ensures Dispose()
// is called even if an exception occurs.
//
// Modern .NET also has IAsyncDisposable + `await using` for async cleanup.

/// <summary>
/// A simple resource tracker that implements IDisposable.
/// Demonstrates the standard Dispose pattern.
/// </summary>
public class ManagedResource : IDisposable
{
    public string Name { get; }
    public bool IsDisposed { get; private set; }

    public ManagedResource(string name) => Name = name;

    public string Read()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        return $"Data from {Name}";
    }

    public void Dispose()
    {
        if (!IsDisposed)
        {
            IsDisposed = true;
            // Release resources here
        }
    }
}

/// <summary>
/// Demonstrates the full IDisposable pattern with a finalizer for unmanaged resources.
/// </summary>
public class UnmanagedResourceWrapper : IDisposable
{
    private bool _disposed;
    public bool IsDisposed => _disposed;
    public nint Handle { get; }

    public UnmanagedResourceWrapper(nint handle) => Handle = handle;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Free managed resources
            }
            // Free unmanaged resources (handle, etc.)
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    ~UnmanagedResourceWrapper() => Dispose(disposing: false);
}

/// <summary>
/// Demonstrates IAsyncDisposable for async cleanup (e.g., flushing buffers).
/// </summary>
public class AsyncResource : IAsyncDisposable
{
    private readonly List<string> _buffer = [];
    public bool IsDisposed { get; private set; }
    public IReadOnlyList<string> FlushedItems { get; private set; } = [];

    public void Write(string item)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        _buffer.Add(item);
    }

    public async ValueTask DisposeAsync()
    {
        if (!IsDisposed)
        {
            // Simulate async flush (e.g., writing buffered data to a stream)
            await Task.Delay(1);
            FlushedItems = [.. _buffer];
            _buffer.Clear();
            IsDisposed = true;
        }
    }
}

/// <summary>
/// A composite disposable that manages multiple child resources.
/// Demonstrates: using pattern for resource aggregation.
/// </summary>
public class ResourcePool : IDisposable
{
    private readonly List<ManagedResource> _resources = [];
    public bool IsDisposed { get; private set; }

    public ManagedResource Acquire(string name)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        var resource = new ManagedResource(name);
        _resources.Add(resource);
        return resource;
    }

    public IReadOnlyList<ManagedResource> Resources => _resources;

    public void Dispose()
    {
        if (!IsDisposed)
        {
            foreach (var resource in _resources)
            {
                resource.Dispose();
            }
            IsDisposed = true;
        }
    }
}

/// <summary>
/// Helper demonstrating using-statement patterns.
/// </summary>
public static class DisposablePatterns
{
    /// <summary>
    /// Classic using-block: resource is disposed at the end of the block.
    /// </summary>
    public static string ReadWithUsingBlock(string resourceName)
    {
        using (var resource = new ManagedResource(resourceName))
        {
            return resource.Read();
        }
        // resource.Dispose() called here, even if Read() throws
    }

    /// <summary>
    /// Using-declaration (C# 8+): resource is disposed at end of enclosing scope.
    /// Cleaner syntax, same guarantee.
    /// </summary>
    public static string ReadWithUsingDeclaration(string resourceName)
    {
        using var resource = new ManagedResource(resourceName);
        return resource.Read();
        // resource.Dispose() called at end of method
    }

    /// <summary>
    /// Async using: for IAsyncDisposable resources.
    /// </summary>
    public static async Task<IReadOnlyList<string>> WriteAndFlushAsync(string[] items)
    {
        await using var resource = new AsyncResource();
        foreach (var item in items)
        {
            resource.Write(item);
        }
        // DisposeAsync() called here, flushing the buffer
        await resource.DisposeAsync();
        return resource.FlushedItems;
    }
}
