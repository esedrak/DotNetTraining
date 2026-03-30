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
/// Integration tests for <c>AccountController</c>.
///
/// Key concepts demonstrated:
///   - <see cref="IClassFixture{TFixture}"/> boots a real in-process server once per class
///     so the full pipeline (routing, middleware, model binding, JSON serialization) runs
///   - <see cref="BankApiFactory"/> swaps the real DB and service with test doubles
///   - <see cref="JwtTokenHelper"/> signs tokens with the same key as <c>AuthMiddleware</c>
///     so the real auth middleware is exercised
///   - <see cref="JsonDocument"/> is used for response body assertions because
///     <c>Account.Balance</c> has a <c>private set</c> that STJ cannot deserialize
/// </summary>
public class AccountControllerIntegrationTests(BankApiFactory factory)
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

    // ── GET /v1/accounts ──────────────────────────────────────────────────────

    [Fact]
    public async Task ListAccounts_Returns200_WithAllAccounts()
    {
        // Arrange
        factory.MockBankService.Reset();

        var accounts = new List<Account>
        {
            new Account("Alice", 100m),
            new Account("Bob", 200m)
        };
        factory.MockBankService
            .Setup(s => s.ListAccountsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(accounts);

        var client = CreateAuthenticatedClient("alice");

        // Act
        var response = await client.GetAsync("/v1/accounts");

        // Assert — full pipeline returns 200 with a JSON array of 2 accounts
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task ListAccounts_Returns401_WhenNoToken()
    {
        // Arrange — no Authorization header
        factory.MockBankService.Reset();

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/v1/accounts");

        // Assert — AuthMiddleware rejects unauthenticated requests
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── GET /v1/accounts/{id} ─────────────────────────────────────────────────

    [Fact]
    public async Task GetAccount_Returns200_WhenAccountExists()
    {
        // Arrange
        factory.MockBankService.Reset();

        var account = new Account("Alice", 500m);
        factory.MockBankService
            .Setup(s => s.GetAccountAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var client = CreateAuthenticatedClient("alice");

        // Act
        var response = await client.GetAsync($"/v1/accounts/{account.Id}");

        // Assert — id, owner, and balance are present in the JSON response
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("id").GetGuid().Should().Be(account.Id);
        doc.RootElement.GetProperty("owner").GetString().Should().Be("Alice");
        doc.RootElement.GetProperty("balance").GetDecimal().Should().Be(500m);
    }

    [Fact]
    public async Task GetAccount_Returns404_WhenNotFound()
    {
        // Arrange
        factory.MockBankService.Reset();

        var id = Guid.NewGuid();
        factory.MockBankService
            .Setup(s => s.GetAccountAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AccountNotFoundException(id));

        var client = CreateAuthenticatedClient("alice");

        // Act
        var response = await client.GetAsync($"/v1/accounts/{id}");

        // Assert — AccountNotFoundException propagates through pipeline → 404
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── POST /v1/accounts ─────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAccount_Returns201_WhenValidRequestWithScope()
    {
        // Arrange
        factory.MockBankService.Reset();

        var created = new Account("Alice", 100m);
        factory.MockBankService
            .Setup(s => s.CreateAccountAsync("Alice", 100m, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var client = CreateAuthenticatedClient("alice", "accounts:write");

        // Act
        var response = await client.PostAsync("/v1/accounts",
            Json(new { owner = "Alice", initialBalance = 100m }));

        // Assert — 201 Created with a Location header pointing to the new account
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain(created.Id.ToString());
    }

    [Fact]
    public async Task CreateAccount_Returns403_WhenScopeMissing()
    {
        // Arrange — authenticated but no "accounts:write" scope
        factory.MockBankService.Reset();

        var client = CreateAuthenticatedClient("alice" /* no scopes */);

        // Act
        var response = await client.PostAsync("/v1/accounts",
            Json(new { owner = "Alice", initialBalance = 100m }));

        // Assert — controller's scope check returns Forbid() → 403 through pipeline
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Service must never be called
        factory.MockBankService.Verify(
            s => s.CreateAccountAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAccount_Returns400_WhenServiceThrowsArgumentException()
    {
        // Arrange — service rejects empty owner name
        factory.MockBankService.Reset();

        factory.MockBankService
            .Setup(s => s.CreateAccountAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Owner cannot be empty."));

        var client = CreateAuthenticatedClient("alice", "accounts:write");

        // Act
        var response = await client.PostAsync("/v1/accounts",
            Json(new { owner = "", initialBalance = 0m }));

        // Assert — controller catches ArgumentException → 400 Bad Request
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Owner cannot be empty.");
    }
}
