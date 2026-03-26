# REST vs gRPC in .NET

Two primary API design philosophies — each with different trade-offs in .NET.

---

## REST (Representational State Transfer)

Resource-oriented. Resources are nouns; HTTP verbs are actions. JSON over HTTP/1.1 or HTTP/2.

```csharp
// ASP.NET Core REST controller
[ApiController]
[Route("v1/[controller]")]
public class AccountsController(IBankService svc) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var account = await svc.GetAccountAsync(id, ct);
        return account is null ? NotFound() : Ok(account);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateAccountRequest req, CancellationToken ct)
    {
        var account = await svc.CreateAccountAsync(req.Owner, req.InitialBalance, ct);
        return CreatedAtAction(nameof(Get), new { id = account.Id }, account);
    }
}
```

**Strengths**: Human-readable, cacheable, firewall-friendly, works with browsers, rich tooling (Swagger/Postman).  
**Weaknesses**: Verbose JSON payload, HTTP/1.1 lacks multiplexing, no built-in streaming.

---

## gRPC

Action-oriented. Typed function calls over HTTP/2 with Protobuf binary encoding.

```protobuf
// bank.proto
syntax = "proto3";
service BankService {
    rpc GetAccount(GetAccountRequest) returns (AccountResponse);
    rpc CreateAccount(CreateAccountRequest) returns (AccountResponse);
    rpc StreamTransactions(StreamRequest) returns (stream TransactionEvent);
}
message GetAccountRequest { string id = 1; }
message AccountResponse { string id = 1; string owner = 2; double balance = 3; }
```

```csharp
// ASP.NET Core gRPC service
public class BankGrpcService(IBankService svc) : BankService.BankServiceBase
{
    public override async Task<AccountResponse> GetAccount(
        GetAccountRequest request, ServerCallContext context)
    {
        var account = await svc.GetAccountAsync(Guid.Parse(request.Id), context.CancellationToken);
        if (account is null)
            throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));
        return new AccountResponse { Id = account.Id.ToString(), Owner = account.Owner, Balance = (double)account.Balance };
    }
}
```

**Strengths**: 5-10× smaller payloads (binary Protobuf), built-in streaming, strong typing enforced by compiler.  
**Weaknesses**: Requires HTTP/2, not human-readable, browser support limited (needs gRPC-Web proxy).

---

## Wire Format Comparison

| | REST | gRPC |
| :--- | :--- | :--- |
| **Format** | JSON (text) | Protobuf (binary) |
| **Payload size** | ~200 bytes | ~40 bytes |
| **Human-readable** | ✅ | ❌ |
| **Schema** | OpenAPI (optional) | `.proto` (required) |
| **Streaming** | SSE / SignalR | Native HTTP/2 streams |
| **Browser support** | ✅ Native | ⚠️ Requires gRPC-Web |
| **Code generation** | NSwag / Kiota | `Grpc.Tools` |

---

## Decision Guide

```
Who is calling this API?
├── Browser / external partner / public → REST (JSON + Swagger UI)
├── Internal .NET service → Both work; gRPC for high-throughput / streaming
└── Mobile app with bandwidth constraints → REST with response projection or gRPC-Web

Does it require streaming?
├── Server-to-client push → SignalR (WebSocket) or SSE (Minimal API)
├── High-throughput bidirectional → gRPC streaming
└── Simple request/response → REST or gRPC unary
```

---

## In This Course

The Bank API uses REST (ASP.NET Core controllers) because:
1. It's consumed by browsers and CLI clients.
2. Human-readable JSON simplifies debugging.
3. Standard tooling (Swagger UI, Postman, curl) works out of the box.

---

## Further Reading

- [gRPC in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/grpc/)
- [Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [Protobuf .NET](https://protobuf.dev/getting-started/dotnetguide/)
