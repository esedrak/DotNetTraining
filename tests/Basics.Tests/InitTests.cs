using DotNetTraining.Basics.Initialization;
using FluentAssertions;

namespace Basics.Tests;

public class InitTests
{
    [Fact]
    public void AppConfig_HasConnectionString_AfterStaticInit()
    {
        AppConfig.ConnectionString.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AppConfig_MaxRetries_HasDefaultValue()
    {
        AppConfig.MaxRetries.Should().BeGreaterThan(0);
    }

    [Fact]
    public void InitializationOrder_LogsInCorrectOrder()
    {
        // Static initializer runs before instance constructor
        InitializationOrder.Log.Should().Contain("static field initialized");
        InitializationOrder.Log.Should().Contain("static constructor called");

        var instance = new InitializationOrder();
        InitializationOrder.Log.Should().Contain("instance constructor called");

        var staticIdx = InitializationOrder.Log.IndexOf("static field initialized");
        var ctorIdx = InitializationOrder.Log.IndexOf("static constructor called");
        staticIdx.Should().BeLessThan(ctorIdx, "static field init runs before static ctor");
    }

    [Fact]
    public void ExpensiveResource_ConnectionString_IsLazy()
    {
        // Accessing .ConnectionString triggers Lazy<T> initialization
        ExpensiveResource.ConnectionString.Should().NotBeNullOrEmpty();
    }
}
