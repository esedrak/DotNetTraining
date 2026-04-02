using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotNetTraining.Basics.Configuration;

// ── Settings records ──────────────────────────────────────────────────────────

/// <summary>
/// Typed settings for a database connection.
/// Bound from the "Database" configuration section via IOptions&lt;DatabaseSettings&gt;.
/// </summary>
public record DatabaseSettings
{
    public string ConnectionString { get; init; } = string.Empty;
    public int MaxPoolSize { get; init; } = 10;
    public bool EnableRetry { get; init; }
}

/// <summary>
/// Feature flag toggles bound from the "FeatureFlags" configuration section.
/// </summary>
public record FeatureFlags
{
    public bool EnableDarkMode { get; init; }
    public bool EnableBetaApi { get; init; }
}

// ── Service that consumes IOptions<DatabaseSettings> ─────────────────────────

/// <summary>
/// Demonstrates injecting typed configuration via IOptions&lt;T&gt;.
/// IOptions&lt;T&gt; is registered as a singleton; values are fixed at startup.
/// </summary>
public class DatabaseService(IOptions<DatabaseSettings> options)
{
    private readonly DatabaseSettings _settings = options.Value;

    public string ConnectionString => _settings.ConnectionString;
    public int MaxPoolSize => _settings.MaxPoolSize;

    /// <summary>Returns true when a non-empty connection string has been configured.</summary>
    public bool IsConfigured => !string.IsNullOrEmpty(_settings.ConnectionString);
}

// ── DI extension: wire up in-memory config + Options ─────────────────────────

/// <summary>
/// Extension methods for registering config-backed options from an in-memory dictionary.
/// Useful in unit tests and quick demos where a full appsettings.json is overkill.
/// </summary>
public static class ConfigurationBuilderExtensions
{
    /// <summary>
    /// Builds an IConfigurationRoot from the supplied key/value pairs and registers
    /// Configure&lt;DatabaseSettings&gt; and Configure&lt;FeatureFlags&gt; on the container.
    /// Nested sections use ':' as the separator:
    ///   "Database:ConnectionString" → DatabaseSettings.ConnectionString
    /// </summary>
    public static IServiceCollection AddInMemorySettings(
        this IServiceCollection services,
        Dictionary<string, string?> values)
    {
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        services.Configure<DatabaseSettings>(config.GetSection("Database"));
        services.Configure<FeatureFlags>(config.GetSection("FeatureFlags"));

        return services;
    }
}

// ── Factory helper ────────────────────────────────────────────────────────────

/// <summary>
/// Creates lightweight IConfiguration instances from dictionaries.
/// Useful in tests and exploratory code where the full DI container is overkill.
/// </summary>
public static class ConfigurationFactory
{
    /// <summary>
    /// Build an IConfiguration backed by in-memory key/value pairs.
    /// Nested values use ':' as the separator:
    ///   { "Database:ConnectionString", "Server=.;Database=dev" }
    /// </summary>
    public static IConfiguration CreateFromDictionary(Dictionary<string, string?> values) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
}
