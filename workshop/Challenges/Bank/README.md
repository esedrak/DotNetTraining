# C# Bank Transfer Quest

Welcome to the **C# Bank Transfer Quest**! In this challenge, you will implement a `POST /v1/transfers` endpoint in a pre-scaffolded ASP.NET Core bank service.

This quest focuses on idiomatic ASP.NET Core controller patterns, OpenTelemetry tracing, structured logging (`ILogger<T>`/Serilog), JWT authentication/authorisation, and controller testing — without getting distracted by database or repository concerns.

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

### Quest 2: Wire the Routes

**File:** [src/Bank.Api/Program.cs](../../../src/Bank.Api/Program.cs)

**Context:**
The ASP.NET Core API uses controller routing via `app.MapControllers()`. Middleware is applied globally in the pipeline order. The `AuthMiddleware` already exists but is commented out. The account controller (`AccountController.cs`) is already wired up and working as your reference.

**Task:**
- Open `src/Bank.Api/Program.cs` — enable `AuthMiddleware` by uncommenting `app.UseMiddleware<AuthMiddleware>()`.
- Open `src/Bank.Api/Controllers/TransferController.cs` — add a `transfers:write` scope requirement to `CreateTransfer`, following the pattern in `AccountController.CreateAccount`.
- Note: `TransferController` contains additional guided `TODO`s for the full action implementation — those are covered in Quest 3.

**Definition of Done:**
- The .NET code compiles successfully.
- You can run the following command with no errors:
  ```bash
  dotnet build src/Bank.Api
  ```

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
- Code compiles without syntax errors or unused variables.
- You can run the following command cleanly:
  ```bash
  dotnet build src/Bank.Api
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

### Quest 4: Write Controller Tests

**File:** [tests/Bank.Tests/Controllers/TransferControllerTests.cs](../../../tests/Bank.Tests/Controllers/TransferControllerTests.cs)

**Context:**
We use `WebApplicationFactory<Program>` for integration tests and `Moq` to unit test controllers in isolation from real dependencies. The existing `BankServiceTests.cs` shows how to construct mocks with `Mock<T>` and write assertions with `FluentAssertions`.

**Task:**
- Open `tests/Bank.Tests/Controllers/TransferControllerTests.cs`.
- Read how the happy path (201) and invalid body (400) cases are constructed in the existing tests.
- Implement the "wrong owner" (403) case: Mock `GetAccountAsync` to return an account owned by `"bob"`, but set the test JWT subject to `"alice"`.
- Implement "insufficient funds" (422): Mock `GetAccountAsync` successfully, but mock `CreateTransferAsync` to throw `InsufficientFundsException`.
- Implement "source account not found" (404): Mock `GetAccountAsync` to throw `AccountNotFoundException`.

**Definition of Done:**
- Run the controller tests:
  ```bash
  dotnet test tests/Bank.Tests --filter "FullyQualifiedName~Transfer"
  ```
- All tests pass successfully, confirming your controller perfectly maps edge cases.
- Finally, verify the entire service is still green:
  ```bash
  dotnet test
  ```

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
- Issue a test JWT using `POST /v1/token` and export the token:
  ```bash
  export BANK_TOKEN="<your-token-here>"
  ```
- Confirm the command returns the account balance:
  ```bash
  dotnet run --project src/Bank.Cli -- account get <account-id>
  ```

### Bonus Quest 2: Transfer Client & CLI

**Context:**
Now that you've seen how the CLI consumes an existing endpoint, it's time to implement both sides from scratch. You'll implement the `transfer create` command that serialises the request body, sets the `Authorization: Bearer` header, and calls `POST /v1/transfers`.

**Task:**
1. **File:** [src/Bank.Cli/Program.cs](../../../src/Bank.Cli/Program.cs) — Implement the `transfer create` command. Parse `--from`, `--to`, and `--amount` options and invoke your newly wired `POST /v1/transfers` endpoint. Look at `account create` to see how we serialise the JSON body and handle error responses.

**Definition of Done:**
- Ensure the Bank API is running (see Bonus Quest 1 setup if needed).
- Check the CLI's built-in documentation:
  ```bash
  dotnet run --project src/Bank.Cli -- --help
  ```
- Execute your CLI and see the balance successfully moved:
  ```bash
  dotnet run --project src/Bank.Cli -- transfer create --from <from-id> --to <to-id> --amount 5000
  ```

## Your Next Step

Mastered the standard API controller? Now it's time to handle long-running, complex business processes that need to survive restarts and failures.

Discover how to orchestrate durable workflows in **[Module 4: Temporal Orchestration](../../../src/Temporal.Worker/README.md)**.

---
[← Back to Challenges Overview](../README.md)

**Good luck! Remember to use `AccountController` as your ultimate reference guide.**
