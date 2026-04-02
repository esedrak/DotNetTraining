using DotNetTraining.Basics.CompositionAndInheritance;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Basics.Tests;

/// <summary>
/// Tests for composition and inheritance — base classes, decorators, embedded resources.
/// </summary>
public class CompositionAndInheritanceTests
{
    // ── TimestampedEntity base (inheritance) ──────────────────────────────────

    [Fact]
    public void BankAccount_HasTimestampedFields_FromBase()
    {
        var account = new BankAccount { Id = Guid.NewGuid(), Owner = "Alice" };

        account.Owner.Should().Be("Alice");
        account.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void BankAccount_Deposit_UpdatesBalance()
    {
        var account = new BankAccount { Id = Guid.NewGuid(), Owner = "Alice" };
        account.Deposit(100m);

        account.Balance.Should().Be(100m);
    }

    // ── InMemoryDataStore ─────────────────────────────────────────────────────

    [Fact]
    public async Task InMemoryDataStore_Save_CanRetrieve()
    {
        var store = new InMemoryDataStore<BankAccount>(a => a.Id);
        var id = Guid.NewGuid();
        var account = new BankAccount { Id = id, Owner = "Bob" };

        await store.SaveAsync(account);

        var found = await store.GetAsync(id);
        found.Should().NotBeNull();
        found!.Owner.Should().Be("Bob");
    }

    [Fact]
    public async Task InMemoryDataStore_GetAsync_ReturnsNull_WhenAbsent()
    {
        var store = new InMemoryDataStore<BankAccount>(a => a.Id);

        var result = await store.GetAsync(Guid.NewGuid());
        result.Should().BeNull();
    }

    // ── LoggingDataStore (decorator / composition) ────────────────────────────

    [Fact]
    public async Task LoggingDataStore_DelegatesTo_InnerStore()
    {
        var inner = new InMemoryDataStore<BankAccount>(a => a.Id);
        var logged = new LoggingDataStore<BankAccount>(inner, NullLogger.Instance);

        var id = Guid.NewGuid();
        var account = new BankAccount { Id = id, Owner = "Charlie" };

        await logged.SaveAsync(account);
        var result = await logged.GetAsync(id);

        result.Should().NotBeNull();
        result!.Owner.Should().Be("Charlie");
    }

    // ── Embedded resources ────────────────────────────────────────────────────

    [Fact]
    public void EmbeddedResources_ListNames_ReturnsCollection()
    {
        var names = EmbeddedResources.ListEmbeddedResources();
        names.Should().NotBeNull();
    }

    [Fact]
    public void EmbeddedResources_ReadNonExistent_ReturnsNull()
    {
        var content = EmbeddedResources.ReadEmbeddedText("this.does.not.exist");
        content.Should().BeNull();
    }
}
