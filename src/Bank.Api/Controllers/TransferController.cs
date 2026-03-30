using System.Diagnostics;
using Bank.Domain;
using Bank.Domain.Exceptions;
using Bank.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bank.Api.Controllers;

[ApiController]
[Authorize]
[Route("v1/transfers")]
public partial class TransferController(IBankService bankService, ILogger<TransferController> logger, ActivitySource activitySource) : ControllerBase
{
    /// <summary>List all transfers.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<Transfer>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListTransfers(CancellationToken ct)
    {
        var transfers = await bankService.ListTransfersAsync(ct);
        return Ok(transfers);
    }

    /// <summary>Get a single transfer by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<Transfer>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransfer(Guid id, CancellationToken ct)
    {
        try
        {
            var transfer = await bankService.GetTransferAsync(id, ct);
            return Ok(transfer);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Transfer '{id}' not found." });
        }
    }

    /// <summary>Create a new transfer between accounts.</summary>
    // Quest 1: Add [ProducesResponseType] attributes for every possible HTTP response.
    [HttpPost]
    [ProducesResponseType<Transfer>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateTransfer([FromBody] CreateTransferRequest request, CancellationToken ct)
    {
        // Quest 2: Scope check.
        var scopes = User.FindAll("scope").Select(c => c.Value);
        if (!scopes.Contains("transfers:write"))
        {
            return Forbid();
        }

        // Quest 3: Implement the controller action
        // TODO 3: Verify ownership — fetch source account, confirm caller owns it
        try
        {
            var sourceAccount = await bankService.GetAccountAsync(request.FromAccountId, ct);
            if (User.Identity?.Name != sourceAccount.Owner)
            {
                return Forbid();
            }
        }
        catch (AccountNotFoundException)
        {
            return NotFound(new { message = $"Account '{request.FromAccountId}' not found." });
        }

        // TODOs 2, 4, 5: Create transfer, trace, map exceptions, log, return 201
        try
        {
            // TODO 2: Start activity BEFORE the service call so it spans the work
            using var activity = activitySource.StartActivity("transfer.create");
            activity?.SetTag("fromAccountId", request.FromAccountId);
            activity?.SetTag("toAccountId", request.ToAccountId);
            activity?.SetTag("amount", request.Amount);

            var transfer = await bankService.CreateTransferAsync(request.FromAccountId, request.ToAccountId, request.Amount, ct);

            // TODO 5: Log success and return 201
            LogTransferCreated(logger, transfer.Id, request.FromAccountId, request.ToAccountId, request.Amount);

            return CreatedAtAction(nameof(GetTransfer), new { id = transfer.Id }, transfer);
        }
        catch (AccountNotFoundException ex)  // TODO 4: → 404
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InsufficientFundsException ex)  // TODO 4: → 422
        {
            return UnprocessableEntity(new { message = ex.Message });
        }
        catch (ArgumentException ex)  // TODO 4: → 400
        {
            logger.LogWarning("Invalid transfer creation request: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        // All other exceptions propagate → global exception handler → 500
    }

    [LoggerMessage(Level = LogLevel.Information,
           Message = "Transfer created: {TransferId} from {From} to {To} for {Amount}")]
    private static partial void LogTransferCreated(
        ILogger logger, Guid transferId, Guid from, Guid to, decimal amount);
}

public record CreateTransferRequest(Guid FromAccountId, Guid ToAccountId, decimal Amount);
