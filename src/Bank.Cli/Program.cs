// Bank CLI — command-line client for the Bank API
//
// Usage:
//   dotnet run --project src/Bank.Cli -- account list
//   dotnet run --project src/Bank.Cli -- account create "Alice" --balance 100
//   dotnet run --project src/Bank.Cli -- account get <id>
//   dotnet run --project src/Bank.Cli -- transfer create --from <id> --to <id> --amount 50
//   dotnet run --project src/Bank.Cli -- durable-transfer create --from <id> --to <id> --amount 5000
//   dotnet run --project src/Bank.Cli -- durable-transfer approve transfer-<guid>
//   dotnet run --project src/Bank.Cli -- durable-transfer reject transfer-<guid>
//   dotnet run --project src/Bank.Cli -- durable-transfer status transfer-<guid>

using System.CommandLine;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

var httpClient = new HttpClient
{
    BaseAddress = new Uri(Environment.GetEnvironmentVariable("BANK_API_URL") ?? "http://localhost:5069/")
};
var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

bool TrySetBearerFromEnv()
{
    var token = Environment.GetEnvironmentVariable("BANK_TOKEN");
    if (string.IsNullOrWhiteSpace(token))
    {
        Console.WriteLine("Error: BANK_TOKEN is not set. Export a token first.");
        return false;
    }

    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    return true;
}

var root = new RootCommand("Bank CLI — interact with the Bank API");

// ── account commands ─────────────────────────────────────────────────────────

var accountCmd = new Command("account", "Manage bank accounts");

// account list
var listCmd = new Command("list", "List all accounts");
listCmd.SetAction(async _ =>
{
    if (!TrySetBearerFromEnv())
    {
        return;
    }

    var r = await httpClient.GetAsync("/v1/accounts");
    var body = await r.Content.ReadAsStringAsync();

    if (r.IsSuccessStatusCode)
    {
        var accounts = JsonSerializer.Deserialize<List<JsonElement>>(body, jsonOptions);
        foreach (var a in accounts ?? [])
        {
            Console.WriteLine(a.GetRawText());
        }
        return;
    }

    if (r.StatusCode == HttpStatusCode.Unauthorized)
    {
        Console.WriteLine("Error 401: token missing or expired. Re-run the token export command.");
        return;
    }

    Console.WriteLine($"Error {(int)r.StatusCode}: {body}");
});
accountCmd.Add(listCmd);

// account get <id>
var idArg = new Argument<Guid>("id") { Description = "Account ID" };
var getCmd = new Command("get", "Get account by ID");
getCmd.Add(idArg);
getCmd.SetAction(async parseResult =>
{
    // TODO (Bonus Quest 1): Check an account balance.
    // 1. Parse the account ID argument: parseResult.GetValue(idArg)
    // 2. Call httpClient.GetAsync($"/v1/accounts/{id}")
    // 3. Print the response body to the console (success or error)
    await Task.CompletedTask;
    Console.WriteLine("Not yet implemented.");
});
accountCmd.Add(getCmd);

// account create <owner> [--balance <amount>]
var ownerArg = new Argument<string>("owner") { Description = "Account owner name" };
var balanceOpt = new Option<decimal>("--balance") { Description = "Initial balance (default 0)" };
var createAccCmd = new Command("create", "Create a new account");
createAccCmd.Add(ownerArg);
createAccCmd.Add(balanceOpt);
createAccCmd.SetAction(async parseResult =>
{
    if (!TrySetBearerFromEnv())
    {
        return;
    }

    var owner = parseResult.GetValue(ownerArg)!;
    var balance = parseResult.GetValue(balanceOpt);
    var r = await httpClient.PostAsJsonAsync("/v1/accounts", new { owner, initialBalance = balance });
    Console.WriteLine(r.IsSuccessStatusCode
        ? await r.Content.ReadAsStringAsync()
        : $"Error {(int)r.StatusCode}: {await r.Content.ReadAsStringAsync()}");
});
accountCmd.Add(createAccCmd);
root.Add(accountCmd);

// ── transfer commands ─────────────────────────────────────────────────────────

var transferCmd = new Command("transfer", "Manage transfers");

// transfer list
var listTxCmd = new Command("list", "List all transfers");
listTxCmd.SetAction(async _ =>
{
    if (!TrySetBearerFromEnv())
    {
        return;
    }

    var txs = await httpClient.GetFromJsonAsync<List<JsonElement>>("/v1/transfers", jsonOptions);
    foreach (var t in txs ?? [])
    {
        Console.WriteLine(t.GetRawText());
    }
});
transferCmd.Add(listTxCmd);

// transfer create --from <id> --to <id> --amount <decimal>
var fromOpt = new Option<Guid>("--from") { Description = "Source account ID", Required = true };
var toOpt = new Option<Guid>("--to") { Description = "Destination account ID", Required = true };
var amountOpt = new Option<decimal>("--amount") { Description = "Transfer amount", Required = true };
var createTxCmd = new Command("create", "Create a new transfer");
createTxCmd.Add(fromOpt);
createTxCmd.Add(toOpt);
createTxCmd.Add(amountOpt);
createTxCmd.SetAction(async parseResult =>
{
    // TODO (Bonus Quest 2): Create an authenticated transfer.
    // 1. Parse --from, --to, --amount options via parseResult.GetValue(...)
    // 2. Read the JWT from the BANK_TOKEN env var; print an error and return if absent
    // 3. Set the Authorization header (hint: AuthenticationHeaderValue in System.Net.Http.Headers):
    //      httpClient.DefaultRequestHeaders.Authorization =
    //          new AuthenticationHeaderValue("Bearer", token);
    // 4. Call httpClient.PostAsJsonAsync("/v1/transfers", new { fromAccountId, toAccountId, amount })
    // 5. Print the response body; print a helpful message if StatusCode is 401 or 403
    await Task.CompletedTask;
    Console.WriteLine("Not yet implemented.");
});
transferCmd.Add(createTxCmd);
root.Add(transferCmd);

// ── durable-transfer commands ─────────────────────────────────────────────────

var durableCmd = new Command("durable-transfer", "Manage durable (Temporal-backed) transfers");

// durable-transfer create --from <id> --to <id> --amount <decimal> [--reference <string>] [--transfer-id <guid>]
var dtFromOpt = new Option<Guid>("--from") { Description = "Source account ID", Required = true };
var dtToOpt = new Option<Guid>("--to") { Description = "Destination account ID", Required = true };
var dtAmountOpt = new Option<decimal>("--amount") { Description = "Transfer amount", Required = true };
var dtReferenceOpt = new Option<string?>("--reference") { Description = "Optional human-readable reference" };
var dtTransferIdOpt = new Option<Guid?>("--transfer-id") { Description = "Optional idempotency key (generated if omitted)" };
var dtCreateCmd = new Command("create", "Start a new durable transfer workflow");
dtCreateCmd.Add(dtFromOpt);
dtCreateCmd.Add(dtToOpt);
dtCreateCmd.Add(dtAmountOpt);
dtCreateCmd.Add(dtReferenceOpt);
dtCreateCmd.Add(dtTransferIdOpt);
dtCreateCmd.SetAction(async parseResult =>
{
    if (!TrySetBearerFromEnv()) return;
    var body = new
    {
        fromAccountId = parseResult.GetValue(dtFromOpt),
        toAccountId   = parseResult.GetValue(dtToOpt),
        amount        = parseResult.GetValue(dtAmountOpt),
        reference     = parseResult.GetValue(dtReferenceOpt),
        transferId    = parseResult.GetValue(dtTransferIdOpt),
    };
    var r = await httpClient.PostAsJsonAsync("/v1/durable-transfers", body);
    Console.WriteLine(r.IsSuccessStatusCode
        ? await r.Content.ReadAsStringAsync()
        : $"Error {(int)r.StatusCode}: {await r.Content.ReadAsStringAsync()}");
});
durableCmd.Add(dtCreateCmd);

// durable-transfer approve <workflow-id>
var dtApproveWorkflowIdArg = new Argument<string>("workflow-id") { Description = "Workflow ID (e.g. transfer-<guid>)" };
var dtApproveCmd = new Command("approve", "Approve a pending high-value transfer");
dtApproveCmd.Add(dtApproveWorkflowIdArg);
dtApproveCmd.SetAction(async parseResult =>
{
    if (!TrySetBearerFromEnv()) return;
    var workflowId = parseResult.GetValue(dtApproveWorkflowIdArg)!;
    var r = await httpClient.PostAsync($"/v1/durable-transfers/{workflowId}/approve", null);
    Console.WriteLine(r.IsSuccessStatusCode
        ? $"Approved: {workflowId}"
        : $"Error {(int)r.StatusCode}: {await r.Content.ReadAsStringAsync()}");
});
durableCmd.Add(dtApproveCmd);

// durable-transfer reject <workflow-id>
var dtRejectWorkflowIdArg = new Argument<string>("workflow-id") { Description = "Workflow ID (e.g. transfer-<guid>)" };
var dtRejectCmd = new Command("reject", "Reject a pending high-value transfer");
dtRejectCmd.Add(dtRejectWorkflowIdArg);
dtRejectCmd.SetAction(async parseResult =>
{
    if (!TrySetBearerFromEnv()) return;
    var workflowId = parseResult.GetValue(dtRejectWorkflowIdArg)!;
    var r = await httpClient.PostAsync($"/v1/durable-transfers/{workflowId}/reject", null);
    Console.WriteLine(r.IsSuccessStatusCode
        ? $"Rejected: {workflowId}"
        : $"Error {(int)r.StatusCode}: {await r.Content.ReadAsStringAsync()}");
});
durableCmd.Add(dtRejectCmd);

// durable-transfer status <workflow-id>
var dtStatusWorkflowIdArg = new Argument<string>("workflow-id") { Description = "Workflow ID (e.g. transfer-<guid>)" };
var dtStatusCmd = new Command("status", "Query the current status of a durable transfer");
dtStatusCmd.Add(dtStatusWorkflowIdArg);
dtStatusCmd.SetAction(async parseResult =>
{
    if (!TrySetBearerFromEnv()) return;
    var workflowId = parseResult.GetValue(dtStatusWorkflowIdArg)!;
    var r = await httpClient.GetAsync($"/v1/durable-transfers/{workflowId}");
    Console.WriteLine(r.IsSuccessStatusCode
        ? await r.Content.ReadAsStringAsync()
        : $"Error {(int)r.StatusCode}: {await r.Content.ReadAsStringAsync()}");
});
durableCmd.Add(dtStatusCmd);

root.Add(durableCmd);

return await root.Parse(args).InvokeAsync();
