using Bank.Domain;
using Bank.Domain.Exceptions;
using Bank.Service;
using Microsoft.AspNetCore.Mvc;

namespace Bank.Api.Controllers;

[ApiController]
[Route("v1/transfers")]
public class TransferController(IBankService bankService, ILogger<TransferController> logger) : ControllerBase
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
    [HttpPost]
    [ProducesResponseType<Transfer>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateTransfer([FromBody] CreateTransferRequest request, CancellationToken ct)
    {
        // TODO 1: Model binding is handled by [ApiController] and [FromBody].
        //         Refer to AccountController.CreateAccount for the error-mapping pattern.

        // TODO 2: Start an OpenTelemetry activity.
        //         Use ActivitySource.StartActivity("transfer.create") and set tags for
        //         fromAccountId, toAccountId, and amount.
        //         See TracingMiddleware.cs for the ActivitySource pattern.

        // TODO 3: Verify ownership.
        //         Extract the caller's identity from HttpContext.User.
        //         Fetch the source account via bankService.GetAccountAsync.
        //         If User.Identity.Name does not match account.Owner, return Forbid().

        // TODO 4: Call the service and map domain exceptions to HTTP responses.
        //         AccountNotFoundException   → NotFound(new { message = ... })
        //         InsufficientFundsException → UnprocessableEntity(new { message = ... })
        //         ArgumentException          → BadRequest(new { message = ... })

        // TODO 5: Log success and return 201 Created.
        //         logger.LogInformation("Transfer created: ...")
        //         return CreatedAtAction(nameof(GetTransfer), new { id = transfer.Id }, transfer)

        // REMOVE THESE LINES when TODOs are implemented:
        _ = logger;
        await Task.CompletedTask;
        return StatusCode(StatusCodes.Status501NotImplemented, new { message = "Not yet implemented." });
    }
}

public record CreateTransferRequest(Guid FromAccountId, Guid ToAccountId, decimal Amount);
