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
        try
        {
            var transfer = await bankService.CreateTransferAsync(
                request.FromAccountId,
                request.ToAccountId,
                request.Amount,
                ct);
            return CreatedAtAction(nameof(GetTransfer), new { id = transfer.Id }, transfer);
        }
        catch (AccountNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InsufficientFundsException ex)
        {
            return UnprocessableEntity(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning("Invalid transfer request: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }
}

public record CreateTransferRequest(Guid FromAccountId, Guid ToAccountId, decimal Amount);
