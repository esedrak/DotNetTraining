# Designing APIs for AI Consumption

The next wave of API consumers are not humans — they are AI agents. The requirements are different.

---

## Human-Driven vs Agent-Driven Consumption

| | Human Developer | AI Agent |
| :--- | :--- | :--- |
| **Reads docs** | Swagger UI, README, blog posts | Machine-readable schema (OpenAPI, MCP manifest) |
| **Calls frequency** | Deliberate, low frequency | Autonomous, potentially hundreds/second |
| **Error handling** | Reads error messages, adjusts | Must parse structured error codes |
| **Safety** | Understands irreversibility | Needs explicit `reversible: false` signals |
| **Discovery** | Browser, docs site | `GET /manifest` or `GET /openapi.json` |
| **Confirmation** | Natural — asks if unsure | Must be programmed to pause for high-risk ops |

---

## Syntax-Centric vs Intent-Centric

**Current APIs** describe *what they accept* (parameters and types).  
**Agent-ready APIs** declare *why they exist*, *when they are safe*, and *what constraints govern them*.

```yaml
# ❌ Syntax-centric — tells an agent nothing useful about risk
post:
  operationId: createTransfer
  requestBody:
    ...

# ✅ Intent-centric — agent understands purpose, risk, and constraints
post:
  operationId: createTransfer
  x-intent: "Transfer funds between two bank accounts"
  x-risk-profile: high
  x-reversible: false
  x-constraints:
    max_amount: 50000
    requires_mfa: true
    idempotency_key_required: true
  x-agent-guidance: >
    This operation is irreversible. Confirm the amount and recipient with the
    user before calling. Always provide an Idempotency-Key.
```

---

## Four Pillars of Agent-Ready API Design

### 1. Semantic Metadata
Every operation should declare its intent and risk in machine-readable form.

```yaml
x-intent: "Create a new bank account"
x-risk-profile: low          # low | medium | high
x-reversible: true
x-agent-guidance: "Safe to call autonomously. Returns the new account ID."
```

### 2. Discoverability
Agents discover your API's capabilities at runtime without reading prose documentation.

```csharp
// Expose a machine-readable manifest
app.MapGet("/mcp/manifest", () => new McpManifest
{
    ServerInfo = new() { Name = "bank-api", Version = "1.0.0" },
    Tools = [
        new McpTool
        {
            Name = "get-account",
            Description = "Retrieve account balance and details by ID",
            InputSchema = JsonSchema.For<GetAccountRequest>(),
            RiskProfile = "low",
            XIntent = "Read account information"
        },
        new McpTool
        {
            Name = "transfer-funds",
            Description = "Transfer money between two accounts",
            InputSchema = JsonSchema.For<CreateTransferRequest>(),
            RiskProfile = "high",
            XIntent = "Move funds between accounts — irreversible",
            XConstraints = new { max_amount = 50000, requires_mfa = true }
        }
    ]
}).AllowAnonymous();
```

### 3. Safety Signals
Agents need explicit signals to know when to pause and confirm with the user.

```yaml
# Operations the agent can call freely
x-risk-profile: low
x-reversible: true

# Operations the agent must log and proceed cautiously
x-risk-profile: medium
x-reversible: true

# Operations the agent MUST confirm with the user first
x-risk-profile: high
x-reversible: false
x-agent-guidance: "STOP. Confirm with user before executing."
```

### 4. Predictable Structure
Consistent response envelopes so agents can parse responses without custom logic per endpoint.

```csharp
// ✅ Agent-friendly response — includes state and reversibility signals
public record TransferResponse(
    Guid TransferId,
    string Status,        // "completed" | "pending" | "failed"
    decimal Amount,
    bool Reversible,      // agents know if they can undo this
    string? IdempotencyKey,
    DateTimeOffset CreatedAt
);

// ✅ Consistent error envelope — machine-readable error codes
public record ApiError(
    string Code,           // "INSUFFICIENT_FUNDS" not "Something went wrong"
    string Message,
    string? TransferId,    // context the agent needs to retry or escalate
    IDictionary<string, string[]>? Details
);
```

---

## Risk Tiering

```
Low Risk (read-only):
  GET /accounts, GET /transactions
  → Agent calls freely, no user confirmation

Medium Risk (reversible state change):
  POST /accounts (create account)
  → Agent logs the action and proceeds

High Risk (irreversible or financial):
  POST /transfers, DELETE /accounts
  → Agent MUST pause and confirm with user
  → Agent includes idempotency key
  → Agent shows user: "About to transfer $500 to Alice. Confirm?"
```

---

## Agent-Safe API Checklist

- [ ] Every operation has a unique, descriptive `operationId`
- [ ] Every operation has `x-intent`
- [ ] Every operation has `x-risk-profile` (low/medium/high)
- [ ] Irreversible operations have `x-reversible: false`
- [ ] Financial/state-changing POSTs support `Idempotency-Key` header
- [ ] Error responses include machine-readable `code` fields (not just messages)
- [ ] A machine-readable manifest is available at `/mcp/manifest` or `/openapi.json`
- [ ] `x-agent-guidance` on high-risk operations

---

## ASP.NET Core: OpenAPI Extensions

```csharp
// Custom OpenAPI document filter to add agent metadata
public class AgentMetadataFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        foreach (var (path, item) in swaggerDoc.Paths)
        {
            foreach (var (method, operation) in item.Operations)
            {
                // Add risk profile based on HTTP method
                var riskProfile = method switch
                {
                    OperationType.Get => "low",
                    OperationType.Post when path.Contains("transfer") => "high",
                    OperationType.Delete => "high",
                    _ => "medium"
                };
                operation.Extensions["x-risk-profile"] = new OpenApiString(riskProfile);
            }
        }
    }
}
```

---

## Further Reading

- [OpenAPI Specification extensions](https://spec.openapis.org/oas/v3.1.0#specification-extensions)
- [Model Context Protocol](https://modelcontextprotocol.io/)
- [Anthropic Claude tool use docs](https://docs.anthropic.com/en/docs/build-with-claude/tool-use)

## Your Next Step

Designing APIs for AI is only half the battle. How do we standardize the communication between these models and our tools?

Discover the standard for AI-tool interaction in: **[Model Context Protocol (MCP)](02-model-context-protocol.md)**.
