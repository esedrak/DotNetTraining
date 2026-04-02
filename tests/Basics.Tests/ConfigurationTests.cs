using DotNetTraining.Basics.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Basics.Tests;

public class ConfigurationTests
{
    [Fact]
    public void ConfigurationFactory_ReadsValue_ByKey()
    {
        var config = ConfigurationFactory.CreateFromDictionary(new()
        {
            ["Database:ConnectionString"] = "Server=.;Database=test"
        });

        config["Database:ConnectionString"].Should().Be("Server=.;Database=test");
    }

    [Fact]
    public void ConfigurationFactory_GetSection_BindsToObject()
    {
        var config = ConfigurationFactory.CreateFromDictionary(new()
        {
            ["Database:ConnectionString"] = "Server=.;Database=test",
            ["Database:MaxPoolSize"] = "15"
        });

        var settings = config.GetSection("Database").Get<DatabaseSettings>();

        settings.Should().NotBeNull();
        settings!.ConnectionString.Should().Be("Server=.;Database=test");
        settings.MaxPoolSize.Should().Be(15);
    }

    [Fact]
    public void IOptions_ReturnsConfiguredValues()
    {
        var config = ConfigurationFactory.CreateFromDictionary(new()
        {
            ["Database:ConnectionString"] = "Server=.;Database=test",
            ["Database:MaxPoolSize"] = "25",
            ["Database:EnableRetry"] = "true"
        });

        var services = new ServiceCollection();
        services.AddOptions();
        services.Configure<DatabaseSettings>(config.GetSection("Database"));

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<DatabaseSettings>>();

        options.Value.ConnectionString.Should().Be("Server=.;Database=test");
        options.Value.MaxPoolSize.Should().Be(25);
        options.Value.EnableRetry.Should().BeTrue();
    }

    [Fact]
    public void DatabaseService_IsConfigured_WhenConnectionStringSet()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddInMemorySettings(new()
        {
            ["Database:ConnectionString"] = "Server=.;Database=prod"
        });
        services.AddTransient<DatabaseService>();

        using var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<DatabaseService>();

        service.IsConfigured.Should().BeTrue();
        service.ConnectionString.Should().Be("Server=.;Database=prod");
    }

    [Fact]
    public void DatabaseService_IsNotConfigured_WhenConnectionStringEmpty()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddInMemorySettings(new Dictionary<string, string?>());
        services.AddTransient<DatabaseService>();

        using var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<DatabaseService>();

        service.IsConfigured.Should().BeFalse();
    }

    [Fact]
    public void FeatureFlags_DefaultToFalse_WhenNotConfigured()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddInMemorySettings(new Dictionary<string, string?>());

        using var provider = services.BuildServiceProvider();
        var flags = provider.GetRequiredService<IOptions<FeatureFlags>>();

        flags.Value.EnableDarkMode.Should().BeFalse();
        flags.Value.EnableBetaApi.Should().BeFalse();
    }
}
