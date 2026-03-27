# Structured Logging in .NET with Serilog

Structured logs are machine-readable JSON — instantly queryable at millions of events per day.

---

## Unstructured vs Structured

```
# ❌ Unstructured — requires grep/awk, breaks at scale
[2024-01-15 10:23:45] INFO Transfer 550e8400 completed: 100.00 from ACC-1 to ACC-2

# ✅ Structured JSON — every field is queryable
{"timestamp":"2024-01-15T10:23:45Z","level":"Information","message":"Transfer completed",
 "transferId":"550e8400","amount":100.00,"fromAccountId":"ACC-1","toAccountId":"ACC-2",
 "traceId":"abc123","requestId":"req-456","durationMs":42}
```

The structured version can answer "total volume transferred per account per day" — the unstructured version cannot.

---

## Setup: Serilog + ASP.NET Core

```xml
<!-- Bank.Api.csproj -->
<PackageReference Include="Serilog.AspNetCore" Version="8.*" />
<PackageReference Include="Serilog.Sinks.Console" Version="6.*" />
<PackageReference Include="Serilog.Sinks.File" Version="5.*" />
```

```csharp
// Program.cs
builder.Host.UseSerilog((context, services, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithEnvironmentName()
        .WriteTo.Console(
            outputTemplate: context.HostingEnvironment.IsDevelopment()
                ? "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
                : "{@l}",  // compact JSON for production
            formatter: context.HostingEnvironment.IsProduction()
                ? new Serilog.Formatting.Compact.CompactJsonFormatter()
                : null)
        .WriteTo.File("logs/bank-api-.log", rollingInterval: RollingInterval.Day);
});
```

---

## Log Levels

| Level | When to Use | Production? |
| :--- | :--- | :--- |
| `Debug` | Detailed diagnostic info, noisy | Never |
| `Information` | Normal business events (request received, transfer created) | Yes |
| `Warning` | Unexpected but handled (retry attempt, deprecated endpoint called) | Yes |
| `Error` | Unhandled failures needing attention | Yes |
| `Fatal` | Application cannot continue | Yes |

---

## Request-Scoped Logger Pattern

Every log line from a request should share the same `requestId` and `traceId`. Use `LogContext.PushProperty` in middleware to enrich all subsequent log calls.

```csharp
// Middleware — enriches every log call within the request pipeline
public class LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = context.TraceIdentifier;
        var traceId = Activity.Current?.TraceId.ToString() ?? requestId;

        using (LogContext.PushProperty("RequestId", requestId))
        using (LogContext.PushProperty("TraceId", traceId))
        using (LogContext.PushProperty("Method", context.Request.Method))
        using (LogContext.PushProperty("Path", context.Request.Path))
        {
            var sw = Stopwatch.StartNew();
            await next(context);

            logger.LogInformation(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds);
        }
    }
}
```

Every log call within the request pipeline — including in services and repositories — automatically carries `RequestId`, `TraceId`, `Method`, and `Path`.

---

## Contextual Logging in Services

```csharp
public class BankService(IBankRepository repo, ILogger<BankService> logger)
{
    public async Task<Transfer> CreateTransferAsync(
        Guid fromId, Guid toId, decimal amount, CancellationToken ct)
    {
        // Uses structured logging — NOT string interpolation
        logger.LogInformation(
            "Creating transfer {Amount} from {FromAccountId} to {ToAccountId}",
            amount, fromId, toId);

        try
        {
            var transfer = await repo.CreateTransferAsync(fromId, toId, amount, ct);

            logger.LogInformation(
                "Transfer {TransferId} completed successfully",
                transfer.Id);

            return transfer;
        }
        catch (InsufficientFundsException ex)
        {
            // Warning — expected business exception, not a bug
            logger.LogWarning(
                "Transfer rejected: insufficient funds in {AccountId}. Available: {Available}, Requested: {Requested}",
                fromId, ex.Available, ex.Requested);
            throw;
        }
    }
}
```

Notice: use structured message templates (`{Amount}`) not string interpolation (`$"{amount}"`). This preserves the values as queryable JSON properties.

---

## Standard Fields to Include

| Context | Fields |
| :--- | :--- |
| **Always** | `timestamp`, `level`, `message`, `traceId`, `requestId` |
| **Per request** | `method`, `path`, `statusCode`, `durationMs`, `userId` |
| **On error** | `exception`, `exceptionType`, `stackTrace` |
| **Business events** | `accountId`, `transferId`, `amount`, `currency` |

---

## appsettings.json Configuration

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information",
        "System": "Warning"
      }
    }
  }
}
```

---

## Never Do This

```csharp
// ❌ No level, no timestamp, not queryable
Console.WriteLine($"Transfer {id} completed");

// ❌ String interpolation destroys structured logging — value is embedded in the string
_logger.LogInformation($"Transfer {id} completed");

// ✅ Structured template — id is a queryable JSON field
_logger.LogInformation("Transfer {TransferId} completed", id);
```

---

## Further Reading

- [Serilog docs](https://serilog.net/)
- [Logging in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging)
- [Seq (local structured log viewer)](https://datalust.co/seq)
- [Elastic Stack (.NET integration)](https://www.elastic.co/guide/en/ecs-logging/dotnet/current/intro.html)

## Your Next Step

Structured logging gives us the "what" and "when," but in a distributed system, we also need the "how it all connects."

Discover how to track requests across service boundaries in: **[Distributed Tracing](04-distributed-tracing.md)**.
