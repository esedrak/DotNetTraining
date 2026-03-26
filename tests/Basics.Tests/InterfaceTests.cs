using DotNetTraining.Basics.Interface;
using FluentAssertions;

namespace Basics.Tests;

public class InterfaceTests
{
    [Fact]
    public void WelcomeService_Greets_UsingFormalGreeter()
    {
        var service = new WelcomeService(new FormalGreeter());
        service.Welcome("Alice").Should().Contain("Alice");
    }

    [Fact]
    public void WelcomeService_Greets_UsingCasualGreeter()
    {
        var service = new WelcomeService(new CasualGreeter());
        service.Welcome("Bob").Should().Contain("Bob");
    }

    [Fact]
    public void InMemoryRepository_StoresAndRetrieves()
    {
        var repo = new InMemoryRepository<string>();
        repo.Add("hello");
        var all = repo.GetAll();
        all.Should().Contain("hello");
    }

    [Fact]
    public void InMemoryRepository_GetById_ReturnsNull_WhenNotFound()
    {
        var repo = new InMemoryRepository<string>();
        repo.GetById(99).Should().BeNull();
    }

    [Fact]
    public void InMemoryRepository_Remove_DeletesItem()
    {
        var repo = new InMemoryRepository<string>();
        repo.Add("hello");
        repo.Remove(1);
        repo.GetById(1).Should().BeNull();
    }

    [Fact]
    public void ManagedResource_Dispose_CanBeCalledTwice()
    {
        var resource = new ManagedResource();
        resource.Dispose();
        var act = () => resource.Dispose();
        act.Should().NotThrow("second Dispose is a no-op");
    }

    [Fact]
    public void ManagedResource_DoWork_ThrowsAfterDispose()
    {
        var resource = new ManagedResource();
        resource.Dispose();
        var act = () => resource.DoWork();
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void ConsoleLogger_DefaultInterfaceMethod_Works()
    {
        ILogger logger = new ConsoleLogger();
        var act = () => logger.LogError("test error");
        act.Should().NotThrow();
    }
}
