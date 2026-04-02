using DotNetTraining.Basics.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Basics.Tests;

public class DependencyInjectionTests
{
    // ── Singleton ─────────────────────────────────────────────────────────────

    [Fact]
    public void Singleton_ReturnsSameInstance_AcrossResolutions()
    {
        using var provider = ServiceProviderFactory.Create();

        var first = provider.GetRequiredService<SingletonOperation>();
        var second = provider.GetRequiredService<SingletonOperation>();

        first.OperationId.Should().Be(second.OperationId,
            "singleton returns the same object on every resolution");
    }

    [Fact]
    public void Singleton_ReturnsSameInstance_AcrossScopes()
    {
        using var provider = ServiceProviderFactory.Create();
        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();

        var fromScope1 = scope1.ServiceProvider.GetRequiredService<SingletonOperation>();
        var fromScope2 = scope2.ServiceProvider.GetRequiredService<SingletonOperation>();

        fromScope1.OperationId.Should().Be(fromScope2.OperationId,
            "singleton is shared across all scopes");
    }

    // ── Scoped ────────────────────────────────────────────────────────────────

    [Fact]
    public void Scoped_ReturnsSameInstance_WithinScope()
    {
        using var provider = ServiceProviderFactory.Create();
        using var scope = provider.CreateScope();

        var first = scope.ServiceProvider.GetRequiredService<ScopedOperation>();
        var second = scope.ServiceProvider.GetRequiredService<ScopedOperation>();

        first.OperationId.Should().Be(second.OperationId,
            "scoped returns the same object within a single scope");
    }

    [Fact]
    public void Scoped_ReturnsDifferentInstance_AcrossScopes()
    {
        using var provider = ServiceProviderFactory.Create();
        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();

        var fromScope1 = scope1.ServiceProvider.GetRequiredService<ScopedOperation>();
        var fromScope2 = scope2.ServiceProvider.GetRequiredService<ScopedOperation>();

        fromScope1.OperationId.Should().NotBe(fromScope2.OperationId,
            "each scope gets its own scoped instance");
    }

    // ── Transient ─────────────────────────────────────────────────────────────

    [Fact]
    public void Transient_ReturnsDifferentInstance_EachTime()
    {
        using var provider = ServiceProviderFactory.Create();

        var first = provider.GetRequiredService<TransientOperation>();
        var second = provider.GetRequiredService<TransientOperation>();

        first.OperationId.Should().NotBe(second.OperationId,
            "transient creates a new instance on every resolution");
    }
}
