using Microsoft.Extensions.DependencyInjection;

namespace DotNetTraining.Basics.DependencyInjection;

// ── Service interface ─────────────────────────────────────────────────────────

/// <summary>
/// Marker interface for operations that expose a unique identifier.
/// Registered with three different lifetimes to show how Singleton / Scoped /
/// Transient behave differently at resolution time.
/// </summary>
public interface IOperation
{
    Guid OperationId { get; }
}

// ── Three lifetime implementations ────────────────────────────────────────────

/// <summary>
/// Singleton — one instance for the entire application lifetime.
/// The Guid is assigned once when the object is first constructed.
/// Every resolution returns the same object.
/// </summary>
public class SingletonOperation : IOperation
{
    public Guid OperationId { get; } = Guid.NewGuid();
}

/// <summary>
/// Scoped — one instance per scope (e.g. one per HTTP request).
/// Two resolutions within the same scope share the same Guid.
/// Two different scopes each receive their own instance.
/// </summary>
public class ScopedOperation : IOperation
{
    public Guid OperationId { get; } = Guid.NewGuid();
}

/// <summary>
/// Transient — a brand-new instance on every resolution.
/// Every call to GetRequiredService&lt;TransientOperation&gt;() returns a new Guid.
/// </summary>
public class TransientOperation : IOperation
{
    public Guid OperationId { get; } = Guid.NewGuid();
}

// ── Consuming service ─────────────────────────────────────────────────────────

/// <summary>
/// Takes each operation type as a distinct constructor parameter so the container
/// injects the correctly-lifetime'd instance for each.
/// This is the preferred pattern: declare dependencies explicitly in the constructor.
/// </summary>
public class OperationService(
    SingletonOperation singleton,
    ScopedOperation scoped,
    TransientOperation transient)
{
    public Guid SingletonId { get; } = singleton.OperationId;
    public Guid ScopedId { get; } = scoped.OperationId;
    public Guid TransientId { get; } = transient.OperationId;
}

// ── Registration extension ────────────────────────────────────────────────────

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Extension method pattern: group related registrations behind a single
    /// well-named method so Program.cs stays clean.
    /// </summary>
    public static IServiceCollection AddOperationServices(this IServiceCollection services)
    {
        services.AddSingleton<SingletonOperation>();
        services.AddScoped<ScopedOperation>();
        services.AddTransient<TransientOperation>();
        services.AddTransient<OperationService>();
        return services;
    }
}

// ── Test helper ───────────────────────────────────────────────────────────────

/// <summary>
/// Builds a standalone <see cref="ServiceProvider"/> for tests and experiments —
/// no host or WebApplication required.
/// </summary>
public static class ServiceProviderFactory
{
    public static ServiceProvider Create()
    {
        var services = new ServiceCollection();
        services.AddOperationServices();
        return services.BuildServiceProvider(validateScopes: true);
    }
}
