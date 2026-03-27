using DotNetTraining.Basics.Disposable;
using FluentAssertions;

namespace Basics.Tests;

public class DisposableTests
{
    // ── ManagedResource (basic IDisposable) ──────────────────────────────────

    [Fact]
    public void ManagedResource_Read_ReturnsData()
    {
        using var resource = new ManagedResource("test-db");
        resource.Read().Should().Be("Data from test-db");
    }

    [Fact]
    public void ManagedResource_Dispose_SetsFlag()
    {
        var resource = new ManagedResource("test-db");
        resource.IsDisposed.Should().BeFalse();

        resource.Dispose();
        resource.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void ManagedResource_Read_ThrowsAfterDispose()
    {
        var resource = new ManagedResource("test-db");
        resource.Dispose();

        var act = () => resource.Read();
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void ManagedResource_DoubleDispose_IsIdempotent()
    {
        var resource = new ManagedResource("test-db");
        resource.Dispose();
        resource.Dispose(); // Should not throw
        resource.IsDisposed.Should().BeTrue();
    }

    // ── UnmanagedResourceWrapper (full Dispose pattern) ──────────────────────

    [Fact]
    public void UnmanagedResourceWrapper_Dispose_SetsFlag()
    {
        var wrapper = new UnmanagedResourceWrapper(42);
        wrapper.Handle.Should().Be(42);

        wrapper.Dispose();
        wrapper.IsDisposed.Should().BeTrue();
    }

    // ── AsyncResource (IAsyncDisposable) ─────────────────────────────────────

    [Fact]
    public async Task AsyncResource_DisposeAsync_FlushesBuffer()
    {
        var resource = new AsyncResource();
        resource.Write("item1");
        resource.Write("item2");

        await resource.DisposeAsync();

        resource.IsDisposed.Should().BeTrue();
        resource.FlushedItems.Should().BeEquivalentTo(["item1", "item2"]);
    }

    [Fact]
    public async Task AsyncResource_Write_ThrowsAfterDispose()
    {
        var resource = new AsyncResource();
        await resource.DisposeAsync();

        var act = () => resource.Write("too late");
        act.Should().Throw<ObjectDisposedException>();
    }

    // ── ResourcePool (composite disposable) ──────────────────────────────────

    [Fact]
    public void ResourcePool_Dispose_DisposesAllChildren()
    {
        var pool = new ResourcePool();
        var r1 = pool.Acquire("db");
        var r2 = pool.Acquire("cache");

        pool.Dispose();

        pool.IsDisposed.Should().BeTrue();
        r1.IsDisposed.Should().BeTrue();
        r2.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void ResourcePool_Acquire_ThrowsAfterDispose()
    {
        var pool = new ResourcePool();
        pool.Dispose();

        var act = () => pool.Acquire("new");
        act.Should().Throw<ObjectDisposedException>();
    }

    // ── DisposablePatterns helper ────────────────────────────────────────────

    [Fact]
    public void ReadWithUsingBlock_DisposesResource()
    {
        var result = DisposablePatterns.ReadWithUsingBlock("file");
        result.Should().Be("Data from file");
    }

    [Fact]
    public void ReadWithUsingDeclaration_DisposesResource()
    {
        var result = DisposablePatterns.ReadWithUsingDeclaration("file");
        result.Should().Be("Data from file");
    }

    [Fact]
    public async Task WriteAndFlushAsync_FlushesOnDispose()
    {
        var result = await DisposablePatterns.WriteAndFlushAsync(["a", "b", "c"]);
        result.Should().BeEquivalentTo(["a", "b", "c"]);
    }
}
