# Model Context Protocol (MCP)

MCP standardises the connection between AI agents (LLMs) and external tools — like USB-C standardises device connections.

---

## The Problem MCP Solves

Without a standard, every LLM needs a custom integration with every tool:

```
Without MCP: N × M integration problem
Claude    ─┬─── Custom integration ──→ Bank API
            ├─── Custom integration ──→ Calendar API
            └─── Custom integration ──→ Jira API
GPT-4     ─┬─── Custom integration ──→ Bank API
            ├─── Custom integration ──→ Calendar API
            └─── Custom integration ──→ Jira API
```

With MCP, each tool exposes one standard interface; every LLM speaks it:

```
With MCP: 1 standard
Claude ─────┐
GPT-4 ──────┼──→ MCP Protocol ──→ Bank MCP Server ──→ Bank API
Gemini ─────┘
```

---

## Three MCP Server Primitives

### Resources (read-only, no side effects)
URI-addressed data the agent can read. Equivalent to GET endpoints.

```csharp
// Resources the agent can read
[McpResource("bank://accounts/{id}", "Get account details")]
public async Task<McpResourceContent> GetAccount(string id, CancellationToken ct)
{
    var account = await _svc.GetAccountAsync(Guid.Parse(id), ct);
    return McpResourceContent.Json(account);
}

[McpResource("bank://accounts/{id}/transactions", "List recent transactions")]
public async Task<McpResourceContent> GetTransactions(string id, CancellationToken ct) { ... }
```

### Tools (named functions, may have side effects)
Typed function calls the agent can invoke. Equivalent to POST/PUT/DELETE endpoints.

```csharp
// Tools the agent can call
[McpTool("get-account", "Retrieve account by ID", RiskProfile = "low")]
public async Task<ToolResult> GetAccount(GetAccountInput input, CancellationToken ct)
{
    var account = await _svc.GetAccountAsync(input.AccountId, ct);
    return ToolResult.Success(account);
}

[McpTool("transfer-funds",
    "Transfer money between accounts — IRREVERSIBLE — confirm with user first",
    RiskProfile = "high",
    Reversible = false)]
public async Task<ToolResult> TransferFunds(TransferInput input, CancellationToken ct)
{
    var transfer = await _svc.CreateTransferAsync(
        input.FromAccountId, input.ToAccountId, input.Amount, ct);
    return ToolResult.Success(transfer);
}
```

### Prompts (parameterised reusable templates)
Pre-built prompt templates the agent can use for common tasks.

```csharp
[McpPrompt("summarise-transactions", "Summarise account transactions for a date range")]
public PromptTemplate SummariseTransactions() => new()
{
    Template = "Summarise the transactions for account {{account_id}} between {{start_date}} and {{end_date}}. " +
               "Group by category and highlight any unusual activity.",
    Parameters = ["account_id", "start_date", "end_date"]
};
```

---

## Wire Protocol

MCP uses **JSON-RPC 2.0** over HTTP (stateless) or stdio (local). Each request is fully self-contained.

```json
// Agent fetches available tools
POST /mcp
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "tools/list",
  "params": {}
}

// MCP server response
{
  "jsonrpc": "2.0",
  "id": 1,
  "result": {
    "tools": [
      {
        "name": "transfer-funds",
        "description": "Transfer money between accounts",
        "inputSchema": { "type": "object", "properties": { ... } },
        "x-risk-profile": "high",
        "x-reversible": false
      }
    ]
  }
}

// Agent calls a tool
POST /mcp
{
  "jsonrpc": "2.0",
  "id": 2,
  "method": "tools/call",
  "params": {
    "name": "transfer-funds",
    "arguments": {
      "fromAccountId": "...",
      "toAccountId": "...",
      "amount": 100.00,
      "idempotencyKey": "550e8400-e29b-41d4-a716-446655440000"
    }
  }
}
```

---

## MCP Request Lifecycle

```
1. Agent fetches manifest: GET /mcp/manifest
   └── Reads tool list, risk profiles, constraints

2. User says: "Transfer $100 to Alice"
   └── Agent matches intent to "transfer-funds" tool

3. Agent checks risk profile: HIGH
   └── Agent asks user: "About to transfer $100 to Alice (account ID: ...). Confirm?"

4. User confirms
   └── Agent calls tools/call with Idempotency-Key

5. MCP server proxies to Bank REST API
   └── Returns result to agent

6. Agent reports to user: "Transfer completed. Reference: TX-550e8400"
```

---

## ASP.NET Core MCP Server (ModelContextProtocol.NET)

```csharp
// Program.cs — install ModelContextProtocol.NET NuGet package
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<BankMcpTools>()
    .WithResources<BankMcpResources>();

// Tool implementations
[McpServerToolType]
public class BankMcpTools(IBankService svc)
{
    [McpServerTool, Description("Get account balance and details by ID")]
    public async Task<string> GetAccount(Guid accountId, CancellationToken ct)
    {
        var account = await svc.GetAccountAsync(accountId, ct);
        return JsonSerializer.Serialize(account);
    }

    [McpServerTool, Description("Transfer funds — HIGH RISK — irreversible, confirm with user")]
    public async Task<string> TransferFunds(
        Guid fromAccountId, Guid toAccountId, decimal amount,
        string idempotencyKey, CancellationToken ct)
    {
        var transfer = await svc.CreateTransferAsync(fromAccountId, toAccountId, amount, ct);
        return JsonSerializer.Serialize(transfer);
    }
}
```

---

## MCP vs REST

| | REST API | MCP Server |
| :--- | :--- | :--- |
| **Designed for** | Human developers | AI agents |
| **Documentation** | Swagger UI (human-readable) | Manifest (machine-readable) |
| **Risk signals** | No standard | `x-risk-profile`, `x-reversible` |
| **Discovery** | Developer portal | `tools/list` at runtime |
| **Protocol** | HTTP REST | JSON-RPC 2.0 |

> MCP does **not** replace REST. It wraps existing REST APIs with a machine-readable capability layer that agents understand.

---

## Further Reading

- [Model Context Protocol Specification](https://spec.modelcontextprotocol.io/)
- [ModelContextProtocol.NET](https://github.com/modelcontextprotocol/csharp-sdk)
- [MCP server examples](https://github.com/modelcontextprotocol/servers)

## Your Next Step

MCP provides the protocol, but how does an agent know which tool to use in the first place?

Learn how agents find the right capabilities in: **[Tool Discovery](03-tool-discovery.md)**.
