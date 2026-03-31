using Bank.Domain;
using Bank.Domain.Exceptions;
using Bank.Repository;
using Bank.Temporal.Activities;
using Bank.Temporal.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Temporalio.Exceptions;

namespace Bank.Temporal.Tests.Activities;

public class TransferActivitiesTests
{
    private readonly Mock<IBankRepository> _mockRepo;
    private readonly TransferActivities _sut;

    public TransferActivitiesTests()
    {
        _mockRepo = new Mock<IBankRepository>();
        var logger = new Mock<ILogger<TransferActivities>>().Object;
        _sut = new TransferActivities(_mockRepo.Object, logger);
    }

    // ── ValidateAccountsActivity ────────────────────────────────────────────

    [Fact]
    public async Task Validate_BothAccountsExist_Succeeds()
    {
        var fromId = Guid.NewGuid();
        var toId = Guid.NewGuid();

        _mockRepo.Setup(r => r.GetAccountAsync(fromId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Account("Alice", 1000m));
        _mockRepo.Setup(r => r.GetAccountAsync(toId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Account("Bob", 0m));

        var input = new ValidateAccountsInput(fromId, toId, 100m);

        var act = async () => await _sut.ValidateAccountsAsync(input);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Validate_FromAccountMissing_ThrowsNonRetryable()
    {
        var fromId = Guid.NewGuid();
        var toId = Guid.NewGuid();

        _mockRepo.Setup(r => r.GetAccountAsync(fromId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        var input = new ValidateAccountsInput(fromId, toId, 100m);

        var act = async () => await _sut.ValidateAccountsAsync(input);

        (await act.Should().ThrowAsync<ApplicationFailureException>())
            .Which.NonRetryable.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ToAccountMissing_ThrowsNonRetryable()
    {
        var fromId = Guid.NewGuid();
        var toId = Guid.NewGuid();

        _mockRepo.Setup(r => r.GetAccountAsync(fromId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Account("Alice", 500m));
        _mockRepo.Setup(r => r.GetAccountAsync(toId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        var input = new ValidateAccountsInput(fromId, toId, 100m);

        var act = async () => await _sut.ValidateAccountsAsync(input);

        (await act.Should().ThrowAsync<ApplicationFailureException>())
            .Which.NonRetryable.Should().BeTrue();
    }

    // ── DebitAccountActivity ────────────────────────────────────────────────

    [Fact]
    public async Task Debit_FirstCall_WithdrawsAndCreatesTransaction()
    {
        var accountId = Guid.NewGuid();
        var transferId = Guid.NewGuid();
        var account = new Account("Alice", 500m);

        _mockRepo.Setup(r => r.GetAccountAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _mockRepo.Setup(r => r.TransactionExistsAsync(accountId, $"debit:{transferId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var input = new DebitInput(accountId, 200m, transferId);

        await _sut.DebitAccountAsync(input);

        account.Balance.Should().Be(300m);
        _mockRepo.Verify(r => r.CreateTransactionAsync(
            It.Is<Transaction>(t =>
                t.AccountId == accountId &&
                t.Amount == 200m &&
                t.Type == TransactionType.Withdrawal &&
                t.Description == $"debit:{transferId}"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Debit_IdempotentRetry_SkipsIfTransactionExists()
    {
        var accountId = Guid.NewGuid();
        var transferId = Guid.NewGuid();

        _mockRepo.Setup(r => r.TransactionExistsAsync(accountId, $"debit:{transferId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var input = new DebitInput(accountId, 200m, transferId);

        await _sut.DebitAccountAsync(input);

        _mockRepo.Verify(r => r.GetAccountAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockRepo.Verify(r => r.CreateTransactionAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Debit_InsufficientFunds_ThrowsNonRetryable()
    {
        var accountId = Guid.NewGuid();
        var transferId = Guid.NewGuid();
        var account = new Account("Alice", 50m);

        _mockRepo.Setup(r => r.GetAccountAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _mockRepo.Setup(r => r.TransactionExistsAsync(accountId, $"debit:{transferId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var input = new DebitInput(accountId, 200m, transferId);

        var act = async () => await _sut.DebitAccountAsync(input);

        (await act.Should().ThrowAsync<ApplicationFailureException>())
            .Which.NonRetryable.Should().BeTrue();
    }

    // ── CreditAccountActivity ───────────────────────────────────────────────

    [Fact]
    public async Task Credit_FirstCall_DepositsAndCreatesTransaction()
    {
        var accountId = Guid.NewGuid();
        var transferId = Guid.NewGuid();
        var account = new Account("Bob", 100m);

        _mockRepo.Setup(r => r.GetAccountAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _mockRepo.Setup(r => r.TransactionExistsAsync(accountId, $"credit:{transferId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var input = new CreditInput(accountId, 200m, transferId);

        await _sut.CreditAccountAsync(input);

        account.Balance.Should().Be(300m);
        _mockRepo.Verify(r => r.CreateTransactionAsync(
            It.Is<Transaction>(t =>
                t.AccountId == accountId &&
                t.Amount == 200m &&
                t.Type == TransactionType.Deposit &&
                t.Description == $"credit:{transferId}"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Credit_IdempotentRetry_SkipsIfTransactionExists()
    {
        var accountId = Guid.NewGuid();
        var transferId = Guid.NewGuid();

        _mockRepo.Setup(r => r.TransactionExistsAsync(accountId, $"credit:{transferId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var input = new CreditInput(accountId, 200m, transferId);

        await _sut.CreditAccountAsync(input);

        _mockRepo.Verify(r => r.GetAccountAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockRepo.Verify(r => r.CreateTransactionAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── RefundDebitActivity ─────────────────────────────────────────────────

    [Fact]
    public async Task Refund_FirstCall_DepositsAndCreatesTransaction()
    {
        var accountId = Guid.NewGuid();
        var transferId = Guid.NewGuid();
        var account = new Account("Alice", 300m);

        _mockRepo.Setup(r => r.GetAccountAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _mockRepo.Setup(r => r.TransactionExistsAsync(accountId, $"refund:{transferId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var input = new RefundInput(accountId, 200m, transferId);

        await _sut.RefundDebitAsync(input);

        account.Balance.Should().Be(500m);
        _mockRepo.Verify(r => r.CreateTransactionAsync(
            It.Is<Transaction>(t =>
                t.AccountId == accountId &&
                t.Amount == 200m &&
                t.Type == TransactionType.Deposit &&
                t.Description == $"refund:{transferId}"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Refund_IdempotentRetry_SkipsIfTransactionExists()
    {
        var accountId = Guid.NewGuid();
        var transferId = Guid.NewGuid();

        _mockRepo.Setup(r => r.TransactionExistsAsync(accountId, $"refund:{transferId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var input = new RefundInput(accountId, 200m, transferId);

        await _sut.RefundDebitAsync(input);

        _mockRepo.Verify(r => r.GetAccountAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockRepo.Verify(r => r.CreateTransactionAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
