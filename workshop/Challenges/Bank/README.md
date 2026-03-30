# C# Bank Transfer Quest

Welcome to the **C# Bank Transfer Quest**! In this challenge, you will implement a `POST /v1/transfers` endpoint in a pre-scaffolded ASP.NET Core bank service.

This quest focuses on idiomatic ASP.NET Core controller patterns, OpenTelemetry tracing, structured logging (`ILogger<T>`/Serilog), JWT authentication/authorisation, and integration testing — without getting distracted by database or repository concerns.

Everything below the API layer is pre-built. If you want to understand how the underlying layers work, check out the [Bank Architecture](../../../src/Bank.Api/README.md).

The **account controller is your fully working reference** — read it, understand every pattern, and then replicate it for transfers.

## Your Quests

Work through the quests in order. Each step builds on the previous one.

### Quest 1: OpenAPI Spec Design

**File:** [docs/openapi/transfers.yaml](../../../docs/openapi/transfers.yaml)

**Context:**
Before writing code, we design the contract. Designing APIs contract-first ensures frontend and backend engineers agree on the API shape without waiting for the implementation.

**Task:**
Complete the partially filled `transfers.yaml` spec.
- Define the request body schema. It needs `fromAccountId` (string/uuid), `toAccountId` (string/uuid), and `amount` (number).
- Define responses for success (201) returning the created transfer object.
- Define responses for various error scenarios (400, 401, 403, 404, 422, 500), following the patterns established in `accounts.yaml`.

**Definition of Done:**
- Your `transfers.yaml` clearly maps out the endpoint, required properties, and all possible HTTP error codes.
- You can compare it side-by-side with `accounts.yaml` and see they share the same consistent structure.
- Using a swagger viewer extension or going to [Swagger Editor](https://editor.swagger.io/), you can validate that your OpenAPI spec is well-formed and renders correctly.
- Solution can be found in [docs/openapi/solution/transfers.yaml](../../../docs/openapi/solution/transfers.yaml).

### Quest 2: Wire Authentication to the Transfer Controller

**File:** [`src/Bank.Api/Controllers/TransferController.cs`](../../../src/Bank.Api/Controllers/TransferController.cs)

**Context:**
`Program.cs` already has JWT Bearer authentication fully configured: `AddJwtBearer` validates every incoming `Authorization: Bearer <token>` header and populates `HttpContext.User` with the caller's claims. `UseAuthentication()` and `UseAuthorization()` are already in the middleware pipeline.

What's missing is applying this to `TransferController`. `[Authorize]` on the controller class tells the authorization middleware to reject unauthenticated requests with `401 Unauthorized` before they reach your action methods. Once inside the action, `User.FindAll("scope")` reads the claims from the validated JWT so you can enforce fine-grained scope checks.

`AccountController` is already complete — read how `[Authorize]` and the scope check are applied there, then replicate the same two patterns for `TransferController`.

**Task:**
1. Add `[Authorize]` to the `TransferController` class, following the pattern in `AccountController`
2. Add a `transfers:write` scope check inside `CreateTransfer`, following the pattern in `AccountController.CreateAccount`
3. Remove the `Skip` attribute from these two tests in `TransferControllerIntegrationTests.cs` and confirm they now pass:
   - `CreateTransfer_Returns401_WhenNoToken`
   - `CreateTransfer_Returns403_WhenScopeMissing`

**Definition of Done:**
- The .NET code compiles successfully:
  ```bash
  dotnet build src/Bank.Api
  ```
- Both un-skipped tests pass:
  ```bash
  dotnet test tests/Bank.Tests --filter "FullyQualifiedName~TransferController"
  ```

<details>
<summary>Common Mistakes &amp; Helpful Hints</summary>

<details>
<summary>How AddJwtBearer and [Authorize] fit together</summary>

`AddJwtBearer` (in `Program.cs`) is the authentication handler — it reads and validates the token. `[Authorize]` (on the controller) is the policy enforcement point — it rejects requests where authentication failed or was absent. They are two separate concerns wired together by `UseAuthentication()` → `UseAuthorization()` in the pipeline. If you add `[Authorize]` but skip `UseAuthentication()`, the user is never populated and every request returns 401 even with a valid token. Open `Program.cs` and read the pipeline order to see how they connect.

</details>

<details>
<summary>Scope claims: FindAll vs FindFirst</summary>

A JWT can carry multiple `scope` claims (one per granted permission). `User.FindFirst("scope")` returns only the first one. `User.FindAll("scope")` returns all of them. Always use `FindAll` for scope checks so you don't silently miss permissions.

```csharp
// WRONG — only checks the first scope claim; breaks with multiple scopes
if (User.FindFirst("scope")?.Value != "transfers:write")
    return Forbid();

// CORRECT — checks all scope claims
var scopes = User.FindAll("scope").Select(c => c.Value);
if (!scopes.Contains("transfers:write"))
    return Forbid();
```

</details>

</details>

### Quest 3: Implement the Controller Action

**File:** [src/Bank.Api/Controllers/TransferController.cs](../../../src/Bank.Api/Controllers/TransferController.cs)

**Context:**
This is the core of the quest. You need to implement the `CreateTransfer` HTTP action. You will extract the request body, start an OpenTelemetry trace, check business authorisation rules (does the caller own the source account?), call the service layer, and accurately map domain exceptions to HTTP responses.

**Task:**
The controller action contains 5 guided `TODO`s. Each `TODO` points directly to the exact line in the reference `AccountController.cs` that demonstrates the pattern.
1. **Parse and Validate:** Use `[FromBody] CreateTransferRequest request` — ASP.NET Core model binding handles this automatically with `[ApiController]`. Return a 400 Bad Request on validation failure using the built-in `ValidationProblem()` response.
2. **OpenTelemetry Trace:** Start an activity using `ActivitySource.StartActivity("transfer.create")` and set tags for `fromAccountId`, `toAccountId`, and `amount`. Ensure the activity is properly disposed with `using`.
3. **Verify Ownership:** Extract the caller's identity via `HttpContext.User`. Fetch the source account using `bankService.GetAccountAsync`. If the caller's `User.Identity.Name` does not match the account owner, return `Forbid()`.
4. **Call Service & Map Exceptions:** Call `bankService.CreateTransferAsync(...)`. Use `try/catch` to map domain exceptions to HTTP responses:
   - `AccountNotFoundException` → 404 `NotFound`
   - `InsufficientFundsException` → 422 `UnprocessableEntity`
   - `ArgumentException` → 400 `BadRequest`
   - Unexpected → re-throw (let the global exception handler deal with it)
5. **Log & Return:** Use `logger.LogInformation` to log the successful transfer (Serilog automatically injects trace IDs via log context enrichment). Return `CreatedAtAction(nameof(GetTransfer), ...)` with a 201 response.

**Definition of Done:**
- Code compiles without syntax errors or unused variables:
  ```bash
  dotnet build src/Bank.Api
  ```
- Remove the `Skip` attribute from these two tests in `TransferControllerIntegrationTests.cs` and confirm they now pass:
  - `CreateTransfer_Returns201_WhenValid`
  - `CreateTransfer_Returns400_WhenArgumentInvalid`
  ```bash
  dotnet test tests/Bank.Tests --filter "FullyQualifiedName~TransferController"
  ```

<details>
<summary>Common Mistakes &amp; Helpful Hints</summary>

<details>
<summary>TODO 1 — Parse &amp; Validate: nothing to write</summary>

`[ApiController]` + `[FromBody]` handle this automatically. If the JSON is malformed or a required field is missing, the framework returns a `400 Bad Request` with a `ValidationProblemDetails` body before your method is ever called. TODO 1 is a reminder that the framework has your back — resist the urge to add manual null-checks or validation code here.

</details>

<details>
<summary>TODO 2 — Activity disposal: always use <code>using</code></summary>

`ActivitySource.StartActivity` returns an `Activity?` which implements `IDisposable`. Disposing it **closes the span** — without it the span never gets an end timestamp and OpenTelemetry exporters may silently drop or misreport it. It can also return `null` when no listener is registered, so use null-conditional operators for tag setting.

```csharp
// BAD — span never closes; .SetTag throws NullReferenceException if no listener
var activity = activitySource.StartActivity("transfer.create");
activity.SetTag("fromAccountId", request.FromAccountId);

// GOOD — span closes on scope exit; tags are no-ops when null
using var activity = activitySource.StartActivity("transfer.create");
activity?.SetTag("fromAccountId", request.FromAccountId);
```

</details>

<details>
<summary>TODO 3 — Ownership check: don't forget <code>await</code></summary>

`bankService.GetAccountAsync` returns `Task<Account>`. Without `await` the variable holds the `Task` itself — not the `Account` — and accessing `.Owner` on a `Task<Account>` is a compile error.

```csharp
// WRONG — sourceAccount is Task<Account>; .Owner does not exist on Task
var sourceAccount = bankService.GetAccountAsync(request.FromAccountId, ct);

// CORRECT — sourceAccount is Account
var sourceAccount = await bankService.GetAccountAsync(request.FromAccountId, ct);
if (User.Identity?.Name != sourceAccount.Owner)
    return Forbid();
```

</details>

<details>
<summary>TODO 4 — 500s: let unexpected exceptions propagate</summary>

Only catch exceptions you can map to a meaningful HTTP response. Anything else should bubble up to the global exception handler middleware (`app.UseExceptionHandler`), which converts it to a `500 Internal Server Error`. Catching bare `Exception` would swallow bugs and make them invisible in traces and logs.

```csharp
catch (AccountNotFoundException ex)   { return NotFound(...); }            // → 404
catch (InsufficientFundsException ex) { return UnprocessableEntity(...); }  // → 422
catch (ArgumentException ex)          { return BadRequest(...); }           // → 400
// all other exceptions propagate → global handler → 500
```

</details>

<details>
<summary>TODO 5 — Structured logging: suppress CA1848 with <code>[LoggerMessage]</code></summary>

Calling `logger.LogInformation(...)` directly triggers warning CA1848 because the arguments are evaluated at the call site even when the log level is disabled. Fix this by declaring a source-generated `[LoggerMessage]` partial method — the compiler wraps it in an `IsEnabled` guard automatically. The class declaration also needs `partial`.

```csharp
// BAD — arguments always evaluated regardless of log level; CA1848 warning
logger.LogInformation("Transfer created: {TransferId} ...", transfer.Id, ...);

// GOOD — compiler emits IsEnabled guard; zero overhead when logging is off
[LoggerMessage(Level = LogLevel.Information,
    Message = "Transfer created: {TransferId} from {From} to {To} for {Amount}")]
private static partial void LogTransferCreated(
    ILogger logger, Guid transferId, Guid from, Guid to, decimal amount);
```

</details>

</details>

### Quest 4: Write Integration Tests

**File:** [tests/Bank.Tests/Controllers/TransferControllerIntegrationTests.cs](../../../tests/Bank.Tests/Controllers/TransferControllerIntegrationTests.cs)

**Context:**
The .NET way to test HTTP controllers is `WebApplicationFactory<Program>` from `Microsoft.AspNetCore.Mvc.Testing`. It boots a real in-process test server and runs your full middleware pipeline — JWT authentication, routing, model binding, exception handling, JSON serialization — on every test request. Tests make calls via `HttpClient`, exactly as a real client would.

This is meaningfully different from calling controller methods directly. A direct call bypasses the pipeline: there is no auth check, no model binding, no response serialization. An `HttpClient` test proves the endpoint actually works end-to-end.

Two helpers are provided:
- **`BankApiFactory`** — a custom `WebApplicationFactory<Program>` that swaps the real Postgres `DbContext` for an in-memory one and replaces `IBankService` with a `Mock<IBankService>`. It exposes `MockBankService` so tests can control what the service returns. Call `factory.MockBankService.Reset()` at the start of each test to clear previous setups.
- **`JwtTokenHelper`** — generates signed JWT tokens using the same key as the app's `AddJwtBearer` configuration. Tests go through the real authentication middleware.

**`AccountControllerIntegrationTests.cs` is your reference** — study the arrange/act/assert pattern and the `JsonDocument` response assertions before writing your own tests. Note: `JsonDocument` is used instead of deserializing to `Account` because `Account.Balance` has a `private set` that the JSON deserializer cannot populate.

**Task:**
- Open `tests/Bank.Tests/Controllers/TransferControllerIntegrationTests.cs`
- Three tests are marked `TODO`. Read the comments in each and implement them following the patterns already in the file:
  - `CreateTransfer_Returns403_WhenCallerIsNotOwner`: mock `GetAccountAsync` to return an account owned by `"bob"`, sign the token for `"alice"`
  - `CreateTransfer_Returns404_WhenSourceAccountNotFound`: mock `GetAccountAsync` to throw `AccountNotFoundException`
  - `CreateTransfer_Returns422_WhenInsufficientFunds`: mock `GetAccountAsync` successfully (alice owns it, low balance), mock `CreateTransferAsync` to throw `InsufficientFundsException`

**Definition of Done:**
- Remove the `[Fact(Skip = ...)]` attribute from each of the three tests you implemented and replace it with `[Fact]`
- All transfer tests pass, 0 skipped:
  ```bash
  dotnet test tests/Bank.Tests --filter "FullyQualifiedName~Transfer"
  ```
- Verify the full suite is green:
  ```bash
  dotnet test
  ```

<details>
<summary>Common Mistakes &amp; Helpful Hints</summary>

<details>
<summary>Always call MockBankService.Reset() first</summary>

`BankApiFactory` is shared across all tests in the class (one server boot). If you forget to reset the mock, a setup from a previous test can bleed into the current one and make it pass or fail for the wrong reason.

```csharp
[Fact]
public async Task MyTest()
{
    factory.MockBankService.Reset(); // always first
    factory.MockBankService.Setup(...).ReturnsAsync(...);
    ...
}
```

</details>

<details>
<summary>JsonDocument for response body assertions</summary>

`Account.Balance` has a `private set`, so `JsonSerializer.Deserialize<Account>(body)` will always give you `Balance = 0` regardless of what the API returned. Use `JsonDocument` to read the serialized output directly:

```csharp
var body = await response.Content.ReadAsStringAsync();
using var doc = JsonDocument.Parse(body);
doc.RootElement.GetProperty("balance").GetDecimal().Should().Be(500m);
```

</details>

</details>

### Bonus Quest 1: Check Account Balance CLI

**Context:**
APIs are useless without clients. Building a strongly-typed .NET CLI client makes integration easy for developer tooling, automation, and other services. Before moving money around, you need to be able to observe account state. This is the simplest introduction to the CLI → API pattern using `System.CommandLine` v3: the HTTP call is a single line, so your only job is to wire the command and display the result.

**Task:**
1. **File:** [src/Bank.Cli/Program.cs](../../../src/Bank.Cli/Program.cs) — Wire up the `account get <id>` CLI command. Parse the `Guid` argument and invoke `GET /v1/accounts/{id}` on the HTTP client.

**Definition of Done:**
- Start the infrastructure:
  ```bash
  make infra-up
  ```
- Apply migrations:
  ```bash
  make db-migrate
  ```
- Build the CLI:
  ```bash
  dotnet build src/Bank.Cli
  ```
- Run the Bank API (in a separate terminal):
  ```bash
  make run-bank-api
  ```
- Issue a dev JWT and export it:
  ```bash
  export BANK_TOKEN=$(curl -s -X POST http://localhost:5069/v1/token \
    -H "Content-Type: application/json" \
    -d '{"userName":"alice","scopes":["accounts:write","transfers:write"]}' \
    | jq -r '.token')
  ```
- Confirm the command returns alice's account balance (`make db-migrate` seeds two fixed accounts):
  ```bash
  # alice  →  00000000-0000-0000-0000-000000000001
  # bob    →  00000000-0000-0000-0000-000000000002
  dotnet run --project src/Bank.Cli -- account get 00000000-0000-0000-0000-000000000001
  ```

### Bonus Quest 2: Authenticated Transfer CLI

**Context:**
The `POST /v1/transfers` endpoint requires a `Authorization: Bearer <token>` header — without it the server returns 401. Unlike `account create`, this command can't just fire a POST and forget: it must read a stored JWT, attach it to the HTTP request, and handle the case where the token is missing or expired.

This introduces `System.Net.Http.Headers.AuthenticationHeaderValue` — the standard .NET type for setting Bearer tokens — and teaches you to make deliberate decisions about *where* in your code to attach credentials.

**Task:**

**File:** [src/Bank.Cli/Program.cs](../../../src/Bank.Cli/Program.cs)

Implement the `transfer create` command. The `--from`, `--to`, and `--amount` options are already wired up; you only need to fill in the `SetAction` body:

1. Parse `--from`, `--to`, `--amount` via `parseResult.GetValue(...)`
2. Read the JWT from the `BANK_TOKEN` environment variable. If the variable is absent, print a clear error message and return — do not call the API.
3. Set the `Authorization` header on the shared `httpClient` instance:
   ```csharp
   httpClient.DefaultRequestHeaders.Authorization =
       new AuthenticationHeaderValue("Bearer", token);
   ```
4. Call `httpClient.PostAsJsonAsync("/v1/transfers", new { fromAccountId, toAccountId, amount })`
5. Print the response body to the console. If the response is `401 Unauthorized`, print a hint that the token may have expired.

**Definition of Done:**
- Ensure the Bank API is running (see Bonus Quest 1 setup if needed).
- Export a valid JWT (same command as Bonus Quest 1):
  ```bash
  export BANK_TOKEN=$(curl -s -X POST http://localhost:5069/v1/token \
    -H "Content-Type: application/json" \
    -d '{"userName":"alice","scopes":["accounts:write","transfers:write"]}' \
    | jq -r '.token')
  ```
- Check the CLI's built-in documentation:
  ```bash
  dotnet run --project src/Bank.Cli -- --help
  ```
- Execute your CLI and see the balance successfully moved (alice → bob):
  ```bash
  dotnet run --project src/Bank.Cli -- transfer create \
    --from 00000000-0000-0000-0000-000000000001 \
    --to   00000000-0000-0000-0000-000000000002 \
    --amount 100
  ```
- Verify a graceful error when the token is missing:
  ```bash
  unset BANK_TOKEN
  dotnet run --project src/Bank.Cli -- transfer create \
    --from 00000000-0000-0000-0000-000000000001 \
    --to   00000000-0000-0000-0000-000000000002 \
    --amount 100
  # Expected: a clear "BANK_TOKEN not set" message, not a 401 from the API
  ```

<details>
<summary>Common Mistakes &amp; Helpful Hints</summary>

<details>
<summary>DefaultRequestHeaders vs per-request headers</summary>

`httpClient.DefaultRequestHeaders.Authorization` sets the header on **every** subsequent request made by that `HttpClient` instance. For a short-lived CLI tool with a single base address this is fine. In a long-running service you would instead pass a header per-request using `HttpRequestMessage` + `SendAsync` to avoid accidentally leaking credentials to other endpoints.

</details>

<details>
<summary>AuthenticationHeaderValue namespace</summary>

`AuthenticationHeaderValue` lives in `System.Net.Http.Headers`. The `using` directive is not in scope by default in top-level programs — add it at the top of `Program.cs`:

```csharp
using System.Net.Http.Headers;
```

</details>

<details>
<summary>Handling 401 vs other error codes</summary>

A 401 means the token was missing or invalid — printing "token expired, re-run POST /v1/token" is genuinely useful. A 403 means the token was valid but lacked the `transfers:write` scope — a different message. Use `r.StatusCode` (a `HttpStatusCode` enum) to branch:

```csharp
if (r.StatusCode == HttpStatusCode.Unauthorized)
    Console.WriteLine("Error: token missing or expired — re-run the curl POST /v1/token command and re-export BANK_TOKEN.");
else if (r.StatusCode == HttpStatusCode.Forbidden)
    Console.WriteLine("Error: token lacks the 'transfers:write' scope.");
else
    Console.WriteLine($"Error {(int)r.StatusCode}: {await r.Content.ReadAsStringAsync()}");
```

</details>

</details>

## Your Next Step

Mastered the standard API controller? Now it's time to handle long-running, complex business processes that need to survive restarts and failures.

Discover how to orchestrate durable workflows in **[Module 4: Temporal Orchestration](../../../src/Temporal.Worker/README.md)**.

---
[← Back to Challenges Overview](../README.md)

**Good luck! Remember to use `AccountController` as your ultimate reference guide.**
