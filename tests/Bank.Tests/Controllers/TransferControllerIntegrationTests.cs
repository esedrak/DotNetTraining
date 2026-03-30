using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Bank.Domain;
using Bank.Domain.Exceptions;
using Bank.Service;
using FluentAssertions;
using Moq;

namespace Bank.Tests.Controllers;

/// <summary>
/// Integration tests for <c>TransferController</c>.
///
/// Key concepts demonstrated:
///   - Real JWT tokens validated by <c>AddJwtBearer</c> — auth runs through the real pipeline
///   - Ownership check (caller must own the source account)
///   - Full exception-to-status-code mapping: 400, 403, 404, 422
///   - <see cref="JsonDocument"/> for response body assertions
/// </summary>
public class TransferControllerIntegrationTests(BankApiFactory factory)
    : IClassFixture<BankApiFactory>
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private HttpClient CreateAuthenticatedClient(string userName, params string[] scopes)
    {
        var client = factory.CreateClient();
        var token = JwtTokenHelper.GenerateToken(userName, scopes);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static StringContent Json(object body) =>
        new(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

    // ── GET /v1/transfers ─────────────────────────────────────────────────────

    [Fact]
    public async Task ListTransfers_Returns200_WithAllTransfers()
    {
        // Arrange
        factory.MockBankService.Reset();

        var transfers = new List<Transfer>
        {
            new Transfer(Guid.NewGuid(), Guid.NewGuid(), 50m),
            new Transfer(Guid.NewGuid(), Guid.NewGuid(), 150m)
        };
        factory.MockBankService
            .Setup(s => s.ListTransfersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transfers);

        var client = CreateAuthenticatedClient("alice");

        // Act
        var response = await client.GetAsync("/v1/transfers");

        // Assert — full pipeline 200 with a JSON array of 2 transfers
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.GetArrayLength().Should().Be(2);
    }

    // ── GET /v1/transfers/{id} ────────────────────────────────────────────────

    [Fact]
    public async Task GetTransfer_Returns200_WhenFound()
    {
        // Arrange
        factory.MockBankService.Reset();

        var fromId = Guid.NewGuid();
        var toId = Guid.NewGuid();
        var transfer = new Transfer(fromId, toId, 75m);

        factory.MockBankService
            .Setup(s => s.GetTransferAsync(transfer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transfer);

        var client = CreateAuthenticatedClient("alice");

        // Act
        var response = await client.GetAsync($"/v1/transfers/{transfer.Id}");

        // Assert — id and amount appear in the JSON body
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("id").GetGuid().Should().Be(transfer.Id);
        doc.RootElement.GetProperty("amount").GetDecimal().Should().Be(75m);
    }

    [Fact]
    public async Task GetTransfer_Returns404_WhenNotFound()
    {
        // Arrange
        factory.MockBankService.Reset();

        var id = Guid.NewGuid();
        factory.MockBankService
            .Setup(s => s.GetTransferAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException($"Transfer '{id}' not found."));

        var client = CreateAuthenticatedClient("alice");

        // Act
        var response = await client.GetAsync($"/v1/transfers/{id}");

        // Assert — KeyNotFoundException → 404
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── POST /v1/transfers ────────────────────────────────────────────────────

    [Fact(Skip = "Quest 3: Remove Skip after implementing CreateTransfer.")]
    public async Task CreateTransfer_Returns201_WhenValid()
    {
        // Arrange — alice owns the source account and has the right scope
        factory.MockBankService.Reset();

        var fromId = Guid.NewGuid();
        var toId = Guid.NewGuid();
        var fromAccount = new Account("alice", 200m);
        var transfer = new Transfer(fromId, toId, 50m);

        factory.MockBankService
            .Setup(s => s.GetAccountAsync(fromId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fromAccount);
        factory.MockBankService
            .Setup(s => s.CreateTransferAsync(fromId, toId, 50m, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transfer);

        var client = CreateAuthenticatedClient("alice", "transfers:write");

        // Act
        var response = await client.PostAsync("/v1/transfers",
            Json(new { fromAccountId = fromId, toAccountId = toId, amount = 50m }));

        // Assert — 201 Created with a Location header pointing to the new transfer
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain(transfer.Id.ToString());
    }

    [Fact(Skip = "Quest 2: Remove Skip after adding [Authorize] to TransferController.")]
    public async Task CreateTransfer_Returns401_WhenNoToken()
    {
        // Arrange — no Authorization header at all
        factory.MockBankService.Reset();

        var client = factory.CreateClient();

        // Act
        var response = await client.PostAsync("/v1/transfers",
            Json(new { fromAccountId = Guid.NewGuid(), toAccountId = Guid.NewGuid(), amount = 50m }));

        // Assert — [Authorize] + AddJwtBearer reject unauthenticated requests
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(Skip = "Quest 2: Remove Skip after adding the transfers:write scope check.")]
    public async Task CreateTransfer_Returns403_WhenScopeMissing()
    {
        // Arrange — authenticated but no "transfers:write" scope
        factory.MockBankService.Reset();

        var client = CreateAuthenticatedClient("alice" /* no scopes */);

        // Act
        var response = await client.PostAsync("/v1/transfers",
            Json(new { fromAccountId = Guid.NewGuid(), toAccountId = Guid.NewGuid(), amount = 50m }));

        // Assert — scope check short-circuits → 403
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        factory.MockBankService.Verify(
            s => s.GetAccountAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(Skip = "TODO (Quest 4): Implement this test.")]
    public async Task CreateTransfer_Returns403_WhenCallerIsNotOwner()
    {
        // TODO (Quest 4): The source account belongs to "bob" but the caller is "alice".
        //
        // Arrange:
        //   - factory.MockBankService.Reset()
        //   - Create a fromId (Guid.NewGuid())
        //   - Mock GetAccountAsync(fromId) to return new Account("bob", 500m)
        //   - Create an authenticated client for "alice" with scope "transfers:write"
        //
        // Act:
        //   - POST /v1/transfers with { fromAccountId = fromId, toAccountId = Guid.NewGuid(), amount = 50m }
        //
        // Assert:
        //   - response.StatusCode should be HttpStatusCode.Forbidden

        throw new NotImplementedException();
    }

    [Fact(Skip = "TODO (Quest 4): Implement this test.")]
    public async Task CreateTransfer_Returns404_WhenSourceAccountNotFound()
    {
        // TODO (Quest 4): The source account does not exist.
        //
        // Arrange:
        //   - factory.MockBankService.Reset()
        //   - Create a fromId (Guid.NewGuid())
        //   - Mock GetAccountAsync(fromId) to throw new AccountNotFoundException(fromId)
        //   - Create an authenticated client for "alice" with scope "transfers:write"
        //
        // Act:
        //   - POST /v1/transfers with { fromAccountId = fromId, toAccountId = Guid.NewGuid(), amount = 50m }
        //
        // Assert:
        //   - response.StatusCode should be HttpStatusCode.NotFound

        throw new NotImplementedException();
    }

    [Fact(Skip = "TODO (Quest 4): Implement this test.")]
    public async Task CreateTransfer_Returns422_WhenInsufficientFunds()
    {
        // TODO (Quest 4): Alice owns the account but does not have enough balance.
        //
        // Arrange:
        //   - factory.MockBankService.Reset()
        //   - Create fromId and toId (Guid.NewGuid() each)
        //   - Mock GetAccountAsync(fromId) to return new Account("alice", 10m)
        //   - Mock CreateTransferAsync(fromId, toId, 500m) to throw
        //       new InsufficientFundsException(fromId, requested: 500m, available: 10m)
        //   - Create an authenticated client for "alice" with scope "transfers:write"
        //
        // Act:
        //   - POST /v1/transfers with { fromAccountId = fromId, toAccountId = toId, amount = 500m }
        //
        // Assert:
        //   - response.StatusCode should be HttpStatusCode.UnprocessableEntity

        throw new NotImplementedException();
    }

    [Fact(Skip = "Quest 3: Remove Skip after implementing CreateTransfer.")]
    public async Task CreateTransfer_Returns400_WhenArgumentInvalid()
    {
        // Arrange — service rejects negative amount
        factory.MockBankService.Reset();

        var fromId = Guid.NewGuid();
        var toId = Guid.NewGuid();
        var fromAccount = new Account("alice", 200m);

        factory.MockBankService
            .Setup(s => s.GetAccountAsync(fromId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fromAccount);
        factory.MockBankService
            .Setup(s => s.CreateTransferAsync(fromId, toId, -1m, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Amount must be positive."));

        var client = CreateAuthenticatedClient("alice", "transfers:write");

        // Act
        var response = await client.PostAsync("/v1/transfers",
            Json(new { fromAccountId = fromId, toAccountId = toId, amount = -1m }));

        // Assert — ArgumentException → 400 Bad Request
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Amount must be positive.");
    }
}
