// Bank CLI — command-line client for the Bank API
//
// Usage:
//   dotnet run --project src/Bank.Cli -- account list
//   dotnet run --project src/Bank.Cli -- account create "Alice" --balance 100
//   dotnet run --project src/Bank.Cli -- account get <id>
//   dotnet run --project src/Bank.Cli -- transfer create --from <id> --to <id> --amount 50

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

return await root.Parse(args).InvokeAsync();
