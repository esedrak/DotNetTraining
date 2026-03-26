using Bank.Domain;
using Bank.Domain.Exceptions;
using Bank.Repository;
using Bank.Service;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Bank.Tests;

public class BankServiceTests
{
    private readonly Mock<IBankRepository> _mockRepo;
    private readonly BankService _sut;

    public BankServiceTests()
    {
        _mockRepo = new Mock<IBankRepository>();
        var logger = new Mock<ILogger<BankService>>().Object;
        _sut = new BankService(_mockRepo.Object, logger);
    }

    // ── CreateAccount ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAccount_ReturnsAccount_WithCorrectOwner()
    {
        // Arrange
        var expected = new Account("Alice", 100m);
        _mockRepo
            .Setup(r => r.CreateAccountAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account a, CancellationToken _) => a);

        // Act
        var result = await _sut.CreateAccountAsync("Alice", 100m);

        // Assert
        result.Owner.Should().Be("Alice");
        result.Balance.Should().Be(100m);
    }

    [Fact]
    public async Task CreateAccount_CallsRepository_Once()
    {
        _mockRepo
            .Setup(r => r.CreateAccountAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account a, CancellationToken _) => a);

        await _sut.CreateAccountAsync("Bob");

        _mockRepo.Verify(r => r.CreateAccountAsync(
            It.Is<Account>(a => a.Owner == "Bob"),
            It.IsAny<CancellationToken>()), Times.Once());
    }

    // ── GetAccount ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAccount_ReturnsAccount_WhenFound()
    {
        var id = Guid.NewGuid();
        var account = new Account("Alice") { };
        _mockRepo.Setup(r => r.GetAccountAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var result = await _sut.GetAccountAsync(id);

        result.Should().BeSameAs(account);
    }

    [Fact]
    public async Task GetAccount_ThrowsAccountNotFoundException_WhenNotFound()
    {
        var id = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetAccountAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        var act = async () => await _sut.GetAccountAsync(id);

        await act.Should().ThrowAsync<AccountNotFoundException>()
            .Where(ex => ex.AccountId == id);
    }

    // ── CreateTransfer ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateTransfer_ThrowsAccountNotFoundException_WhenFromAccountMissing()
    {
        var fromId = Guid.NewGuid();
        var toId = Guid.NewGuid();

        _mockRepo.Setup(r => r.GetAccountAsync(fromId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        var act = async () => await _sut.CreateTransferAsync(fromId, toId, 50m);

        await act.Should().ThrowAsync<AccountNotFoundException>()
            .Where(ex => ex.AccountId == fromId);
    }

    [Fact]
    public async Task CreateTransfer_ThrowsInsufficientFunds_WhenBalanceTooLow()
    {
        var from = new Account("Alice", 10m);
        var to = new Account("Bob", 0m);

        _mockRepo.Setup(r => r.GetAccountAsync(from.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(from);
        _mockRepo.Setup(r => r.GetAccountAsync(to.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(to);

        var act = async () => await _sut.CreateTransferAsync(from.Id, to.Id, 100m);

        await act.Should().ThrowAsync<InsufficientFundsException>()
            .Where(ex => ex.AccountId == from.Id);
    }
}
