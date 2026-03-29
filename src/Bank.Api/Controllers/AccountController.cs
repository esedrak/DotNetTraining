using Bank.Domain;
using Bank.Domain.Exceptions;
using Bank.Service;
using Microsoft.AspNetCore.Mvc;

namespace Bank.Api.Controllers;

[ApiController]
[Route("v1/accounts")]
public class AccountController(IBankService bankService, ILogger<AccountController> logger) : ControllerBase
{
    /// <summary>List all accounts.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<Account>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAccounts(CancellationToken ct)
    {
        var accounts = await bankService.ListAccountsAsync(ct);
        return Ok(accounts);
    }

    /// <summary>Get a single account by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<Account>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccount(Guid id, CancellationToken ct)
    {
        try
        {
            var account = await bankService.GetAccountAsync(id, ct);
            return Ok(account);
        }
        catch (AccountNotFoundException)
        {
            return NotFound(new { message = $"Account '{id}' not found." });
        }
    }

    /// <summary>Create a new account.</summary>
    [HttpPost]
    [ProducesResponseType<Account>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request, CancellationToken ct)
    {
        var scopes = User.FindAll("scope").Select(c => c.Value);
        if (!scopes.Contains("accounts:write"))
        {
            return Forbid();
        }

        try
        {
            var account = await bankService.CreateAccountAsync(request.Owner, request.InitialBalance, ct);
            return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, account);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning("Invalid account creation request: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }
}

public record CreateAccountRequest(string Owner, decimal InitialBalance = 0m);
