using Bank.Domain;
using Bank.Temporal.Models;
using Bank.Temporal.Workflows;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Temporalio.Client;
using Temporalio.Exceptions;

namespace Bank.Api.Controllers;

[ApiController]
[Authorize]
[Route("v1/durable-transfers")]
public class DurableTransferController(
    ITemporalClient temporal,
    ILogger<DurableTransferController> logger) : ControllerBase
{
    /// <summary>Start a new durable transfer workflow.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateTransfer(
        [FromBody] CreateDurableTransferRequest request, CancellationToken ct)
    {
        var scopes = User.FindAll("scope").Select(c => c.Value);
        if (!scopes.Contains("transfers:write"))
        {
            return Forbid();
        }

        var transferId = request.TransferId ?? Guid.NewGuid();
        var workflowId = $"transfer-{transferId}";

        var input = new TransferInput(
            transferId,
            request.FromAccountId,
            request.ToAccountId,
            request.Amount,
            request.Reference);

        try
        {
            await temporal.StartWorkflowAsync(
                (DurableTransferWorkflow wf) => wf.RunAsync(input),
                new WorkflowOptions
                {
                    Id = workflowId,
                    TaskQueue = "durable-transfers",
                });

            logger.LogInformation(
                "Started durable transfer {WorkflowId} for transferId {TransferId}",
                workflowId, transferId);

            return Accepted(new { workflowId, transferId });
        }
        catch (WorkflowAlreadyStartedException)
        {
            return Conflict(new { message = $"Transfer '{transferId}' is already in progress.", workflowId });
        }
    }

    /// <summary>Approve a pending high-value transfer.</summary>
    [HttpPost("{workflowId}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Approve(string workflowId)
    {
        var scopes = User.FindAll("scope").Select(c => c.Value);
        if (!scopes.Contains("transfers:write"))
        {
            return Forbid();
        }

        var handle = temporal.GetWorkflowHandle<DurableTransferWorkflow>(workflowId);
        await handle.SignalAsync(wf => wf.ApproveAsync());

        logger.LogInformation("Approved durable transfer {WorkflowId}", workflowId);
        return NoContent();
    }

    /// <summary>Reject a pending high-value transfer.</summary>
    [HttpPost("{workflowId}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Reject(string workflowId)
    {
        var scopes = User.FindAll("scope").Select(c => c.Value);
        if (!scopes.Contains("transfers:write"))
        {
            return Forbid();
        }

        var handle = temporal.GetWorkflowHandle<DurableTransferWorkflow>(workflowId);
        await handle.SignalAsync(wf => wf.RejectAsync());

        logger.LogInformation("Rejected durable transfer {WorkflowId}", workflowId);
        return NoContent();
    }

    /// <summary>Query the current status of a durable transfer.</summary>
    [HttpGet("{workflowId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatus(string workflowId)
    {
        var handle = temporal.GetWorkflowHandle<DurableTransferWorkflow>(workflowId);
        var status = await handle.QueryAsync<TransferStatus>(wf => wf.GetStatus());
        return Ok(new { workflowId, status = status.ToString() });
    }
}

public record CreateDurableTransferRequest(
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    string? Reference = null,
    Guid? TransferId = null
);
