using Bank.Domain;
using Bank.Temporal.Models;
using Bank.Temporal.Workflows;
using FluentAssertions;
using Temporalio.Client;
using Temporalio.Exceptions;
using Temporalio.Testing;
using Temporalio.Worker;

namespace Bank.Temporal.Tests.Workflows;

/// <summary>
/// Tests for DurableTransferWorkflow using Temporal's time-skipping test environment.
/// Activities are mocked to isolate workflow orchestration logic.
/// </summary>
public class DurableTransferWorkflowTests : IAsyncLifetime
{
    private WorkflowEnvironment _env = null!;
    private readonly MockTransferActivities _mockActivities = new();

    public async Task InitializeAsync()
    {
        _env = await WorkflowEnvironment.StartTimeSkippingAsync();
    }

    public async Task DisposeAsync()
    {
        await _env.DisposeAsync();
    }

    private async Task<TransferResult> ExecuteWorkflowAsync(
        TransferInput input,
        Func<WorkflowHandle<DurableTransferWorkflow>, Task>? duringExecution = null)
    {
        const string taskQueue = "test-durable-transfers";
        using var worker = new TemporalWorker(
            _env.Client,
            new TemporalWorkerOptions(taskQueue)
                .AddWorkflow<DurableTransferWorkflow>()
                .AddAllActivities(_mockActivities));

        return await worker.ExecuteAsync(async () =>
        {
            if (duringExecution is null)
            {
                return await _env.Client.ExecuteWorkflowAsync(
                    (DurableTransferWorkflow wf) => wf.RunAsync(input),
                    new WorkflowOptions
                    {
                        Id = $"transfer-{input.TransferId}",
                        TaskQueue = taskQueue,
                    });
            }

            var handle = await _env.Client.StartWorkflowAsync(
                (DurableTransferWorkflow wf) => wf.RunAsync(input),
                new WorkflowOptions
                {
                    Id = $"transfer-{input.TransferId}",
                    TaskQueue = taskQueue,
                });

            await duringExecution(handle);
            return await handle.GetResultAsync();
        });
    }

    // ── Happy Path ──────────────────────────────────────────────────────────

    [Fact]
    public async Task SmallTransfer_CompletesAutomatically_WithoutApproval()
    {
        var input = new TransferInput(
            TransferId: Guid.NewGuid(),
            FromAccountId: Guid.NewGuid(),
            ToAccountId: Guid.NewGuid(),
            Amount: 500m);

        var result = await ExecuteWorkflowAsync(input);

        result.Status.Should().Be(TransferStatus.Completed);
        result.TransferId.Should().Be(input.TransferId);
        result.FailureReason.Should().BeNull();

        _mockActivities.ValidateCalls.Should().Be(1);
        _mockActivities.DebitCalls.Should().Be(1);
        _mockActivities.CreditCalls.Should().Be(1);
        _mockActivities.RefundCalls.Should().Be(0);
    }

    [Fact]
    public async Task ExactlyThresholdAmount_CompletesWithoutApproval()
    {
        var input = new TransferInput(
            TransferId: Guid.NewGuid(),
            FromAccountId: Guid.NewGuid(),
            ToAccountId: Guid.NewGuid(),
            Amount: 1000m);

        var result = await ExecuteWorkflowAsync(input);

        result.Status.Should().Be(TransferStatus.Completed);
        _mockActivities.DebitCalls.Should().Be(1);
        _mockActivities.CreditCalls.Should().Be(1);
    }

    // ── Approval Path ───────────────────────────────────────────────────────

    [Fact]
    public async Task LargeTransfer_WaitsForApproval_ThenCompletes()
    {
        var input = new TransferInput(
            TransferId: Guid.NewGuid(),
            FromAccountId: Guid.NewGuid(),
            ToAccountId: Guid.NewGuid(),
            Amount: 5000m);

        var result = await ExecuteWorkflowAsync(input, async handle =>
        {
            // Allow workflow to reach the approval gate
            await Task.Delay(500);
            await handle.SignalAsync(wf => wf.ApproveAsync());
        });

        result.Status.Should().Be(TransferStatus.Completed);
        result.FailureReason.Should().BeNull();

        _mockActivities.ValidateCalls.Should().Be(1);
        _mockActivities.DebitCalls.Should().Be(1);
        _mockActivities.CreditCalls.Should().Be(1);
    }

    // ── Rejection Path ──────────────────────────────────────────────────────

    [Fact]
    public async Task LargeTransfer_RejectedBySignal_FailsImmediately()
    {
        var input = new TransferInput(
            TransferId: Guid.NewGuid(),
            FromAccountId: Guid.NewGuid(),
            ToAccountId: Guid.NewGuid(),
            Amount: 2000m);

        var result = await ExecuteWorkflowAsync(input, async handle =>
        {
            await Task.Delay(500);
            await handle.SignalAsync(wf => wf.RejectAsync());
        });

        result.Status.Should().Be(TransferStatus.Failed);
        result.FailureReason.Should().Contain("rejected");

        _mockActivities.ValidateCalls.Should().Be(1);
        _mockActivities.DebitCalls.Should().Be(0, "debit should not execute after rejection");
        _mockActivities.CreditCalls.Should().Be(0);
    }

    // ── Timeout Path ────────────────────────────────────────────────────────

    [Fact]
    public async Task LargeTransfer_NoSignalReceived_TimesOutAfter24Hours()
    {
        var input = new TransferInput(
            TransferId: Guid.NewGuid(),
            FromAccountId: Guid.NewGuid(),
            ToAccountId: Guid.NewGuid(),
            Amount: 1500m);

        // Time-skipping environment will fast-forward through the 24h wait
        var result = await ExecuteWorkflowAsync(input);

        result.Status.Should().Be(TransferStatus.Failed);
        result.FailureReason.Should().Contain("timed out");

        _mockActivities.DebitCalls.Should().Be(0, "debit should not execute after timeout");
        _mockActivities.CreditCalls.Should().Be(0);
    }

    // ── Compensation Path ───────────────────────────────────────────────────

    [Fact]
    public async Task CreditFailure_TriggersRefundCompensation()
    {
        _mockActivities.CreditShouldFail = true;

        var input = new TransferInput(
            TransferId: Guid.NewGuid(),
            FromAccountId: Guid.NewGuid(),
            ToAccountId: Guid.NewGuid(),
            Amount: 250m);

        var result = await ExecuteWorkflowAsync(input);

        result.Status.Should().Be(TransferStatus.Failed);
        result.FailureReason.Should().Contain("Credit failed");

        _mockActivities.ValidateCalls.Should().Be(1);
        _mockActivities.DebitCalls.Should().Be(1, "debit should have been attempted");
        _mockActivities.CreditCalls.Should().Be(1, "credit should have been attempted");
        _mockActivities.RefundCalls.Should().Be(1, "refund must be called to compensate the debit");
    }

    [Fact]
    public async Task CreditFailure_RefundReceivesCorrectFromAccountId()
    {
        _mockActivities.CreditShouldFail = true;

        var fromId = Guid.NewGuid();
        var input = new TransferInput(
            TransferId: Guid.NewGuid(),
            FromAccountId: fromId,
            ToAccountId: Guid.NewGuid(),
            Amount: 300m);

        await ExecuteWorkflowAsync(input);

        _mockActivities.LastRefundInput.Should().NotBeNull();
        _mockActivities.LastRefundInput!.AccountId.Should().Be(fromId);
        _mockActivities.LastRefundInput!.TransferId.Should().Be(input.TransferId);
        _mockActivities.LastRefundInput!.Amount.Should().Be(300m);
    }

    // ── Idempotency (workflow level) ────────────────────────────────────────

    [Fact]
    public async Task SameWorkflowId_DoesNotCreateDuplicateExecution()
    {
        var transferId = Guid.NewGuid();
        var input = new TransferInput(
            TransferId: transferId,
            FromAccountId: Guid.NewGuid(),
            ToAccountId: Guid.NewGuid(),
            Amount: 100m);

        const string taskQueue = "test-durable-transfers";
        using var worker = new TemporalWorker(
            _env.Client,
            new TemporalWorkerOptions(taskQueue)
                .AddWorkflow<DurableTransferWorkflow>()
                .AddAllActivities(_mockActivities));

        await worker.ExecuteAsync(async () =>
        {
            var workflowId = $"transfer-{transferId}";

            var handle1 = await _env.Client.StartWorkflowAsync(
                (DurableTransferWorkflow wf) => wf.RunAsync(input),
                new WorkflowOptions { Id = workflowId, TaskQueue = taskQueue });

            // Second start with same ID while running should throw WorkflowAlreadyStartedException
            var act = async () => await _env.Client.StartWorkflowAsync(
                (DurableTransferWorkflow wf) => wf.RunAsync(input),
                new WorkflowOptions { Id = workflowId, TaskQueue = taskQueue });

            await act.Should().ThrowAsync<WorkflowAlreadyStartedException>();

            var result = await handle1.GetResultAsync();
            result.TransferId.Should().Be(transferId);
            _mockActivities.DebitCalls.Should().Be(1, "only one execution should have run");
        });
    }
}

/// <summary>
/// Mock activities that track invocations and allow configurable failures.
/// Registered on the test worker in place of real TransferActivities.
/// Method signatures must match the real [Activity] methods exactly.
/// </summary>
public class MockTransferActivities
{
    public int ValidateCalls { get; private set; }
    public int DebitCalls { get; private set; }
    public int CreditCalls { get; private set; }
    public int RefundCalls { get; private set; }

    public bool CreditShouldFail { get; set; }
    public RefundInput? LastRefundInput { get; private set; }

    [Temporalio.Activities.Activity]
    public Task ValidateAccountsAsync(ValidateAccountsInput input)
    {
        ValidateCalls++;
        return Task.CompletedTask;
    }

    [Temporalio.Activities.Activity]
    public Task DebitAccountAsync(DebitInput input)
    {
        DebitCalls++;
        return Task.CompletedTask;
    }

    [Temporalio.Activities.Activity]
    public Task CreditAccountAsync(CreditInput input)
    {
        CreditCalls++;
        if (CreditShouldFail)
        {
            throw new ApplicationFailureException(
                "Simulated credit failure: destination account closed",
                nonRetryable: true);
        }
        return Task.CompletedTask;
    }

    [Temporalio.Activities.Activity]
    public Task RefundDebitAsync(RefundInput input)
    {
        RefundCalls++;
        LastRefundInput = input;
        return Task.CompletedTask;
    }
}
