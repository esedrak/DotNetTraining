using Bank.Domain;
using Bank.Temporal.Activities;
using Bank.Temporal.Models;
using Temporalio.Workflows;

namespace Bank.Temporal.Workflows;

[Workflow]
public class DurableTransferWorkflow
{
    private bool _approved;
    private bool _rejected;

    private static readonly ActivityOptions DefaultActivityOptions = new()
    {
        StartToCloseTimeout = TimeSpan.FromSeconds(30),
        RetryPolicy = new()
        {
            MaximumAttempts = 5,
            InitialInterval = TimeSpan.FromSeconds(1),
            BackoffCoefficient = 2.0f,
        },
    };

    [WorkflowRun]
    public async Task<TransferResult> RunAsync(TransferInput input)
    {
        // Step 1 — Validate accounts exist and input is sane
        await Workflow.ExecuteActivityAsync(
            (TransferActivities a) => a.ValidateAccountsAsync(
                new ValidateAccountsInput(input.FromAccountId, input.ToAccountId, input.Amount)),
            new()
            {
                StartToCloseTimeout = TimeSpan.FromSeconds(10),
                RetryPolicy = new()
                {
                    MaximumAttempts = 3,
                    InitialInterval = TimeSpan.FromSeconds(1),
                    BackoffCoefficient = 2.0f,
                },
            });

        // Step 2 — Approval gate for high-value transfers
        if (input.Amount > 1000m)
        {
            var conditionMet = await Workflow.WaitConditionAsync(
                () => _approved || _rejected,
                TimeSpan.FromHours(24));

            if (!conditionMet)
            {
                return new TransferResult(input.TransferId, TransferStatus.Failed,
                    "Approval timed out after 24 hours");
            }

            if (_rejected)
            {
                return new TransferResult(input.TransferId, TransferStatus.Failed,
                    "Transfer rejected by approver");
            }
        }

        // Step 3 — Debit the source account
        await Workflow.ExecuteActivityAsync(
            (TransferActivities a) => a.DebitAccountAsync(
                new DebitInput(input.FromAccountId, input.Amount, input.TransferId)),
            DefaultActivityOptions);

        // Step 4 — Credit the destination account (with compensation on failure)
        try
        {
            await Workflow.ExecuteActivityAsync(
                (TransferActivities a) => a.CreditAccountAsync(
                    new CreditInput(input.ToAccountId, input.Amount, input.TransferId)),
                DefaultActivityOptions);
        }
        catch
        {
            // Step 5 — Compensation: reverse the debit
            // Uses CancellationToken.None so compensation completes even if the workflow is cancelled
            await Workflow.ExecuteActivityAsync(
                (TransferActivities a) => a.RefundDebitAsync(
                    new RefundInput(input.FromAccountId, input.Amount, input.TransferId)),
                new()
                {
                    StartToCloseTimeout = TimeSpan.FromSeconds(30),
                    CancellationToken = CancellationToken.None,
                    RetryPolicy = new()
                    {
                        MaximumAttempts = 10,
                        InitialInterval = TimeSpan.FromSeconds(2),
                        BackoffCoefficient = 2.0f,
                    },
                });

            return new TransferResult(input.TransferId, TransferStatus.Failed,
                "Credit failed; debit reversed");
        }

        return new TransferResult(input.TransferId, TransferStatus.Completed);
    }

    [WorkflowSignal]
    public Task ApproveAsync()
    {
        _approved = true;
        return Task.CompletedTask;
    }

    [WorkflowSignal]
    public Task RejectAsync()
    {
        _rejected = true;
        return Task.CompletedTask;
    }

    [WorkflowQuery]
    public TransferStatus GetStatus() =>
        _approved ? TransferStatus.Completed : TransferStatus.Pending;
}
