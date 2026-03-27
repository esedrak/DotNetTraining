# Tool Discovery for AI Agents

Without discovery, agents must have tools hardcoded at build time. With discovery, agents fetch available capabilities at runtime and adapt.

---

## The Problem Without Discovery

```csharp
// ❌ Hardcoded tool list — requires agent redeployment to add new tools
var tools = new[]
{
    new Tool("get-account", ...),
    new Tool("transfer-funds", ...),
    // Adding "create-account" requires redeploying the agent
};
```

## With Discovery

```csharp
// ✅ Dynamic discovery — new tools appear without agent changes
var manifest = await httpClient.GetFromJsonAsync<McpManifest>("/mcp/manifest");
var availableTools = manifest.Tools.ToDictionary(t => t.Name);
// "create-account" appears in the manifest on next server deploy — agent picks it up
```

---

## The Manifest

`GET /mcp/manifest` returns a complete description of everything the server can do.

```csharp
app.MapGet("/mcp/manifest", (IBankService svc) => new McpManifest
{
    ServerInfo = new McpServerInfo
    {
        Name = "bank-api",
        Version = "1.0.0",
        Description = "Bank API MCP server — accounts and transfers"
    },
    Resources = [
        new McpResource
        {
            UriTemplate = "bank://accounts/{id}",
            Name = "get-account",
            Description = "Retrieve account details by ID",
            XIntent = "Read account balance and metadata",
            XRiskProfile = "low"
        }
    ],
    Tools = [
        new McpTool
        {
            Name = "get-account",
            Description = "Retrieve account balance and details by ID",
            InputSchema = new
            {
                type = "object",
                required = new[] { "accountId" },
                properties = new
                {
                    accountId = new { type = "string", format = "uuid",
                        description = "The account UUID" }
                }
            },
            XIntent = "Read account information",
            XRiskProfile = "low",
            XAgentGuidance = "Safe to call autonomously."
        },
        new McpTool
        {
            Name = "transfer-funds",
            Description = "Transfer money between two bank accounts",
            InputSchema = new
            {
                type = "object",
                required = new[] { "fromAccountId", "toAccountId", "amount", "idempotencyKey" },
                properties = new
                {
                    fromAccountId = new { type = "string", format = "uuid" },
                    toAccountId = new { type = "string", format = "uuid" },
                    amount = new { type = "number", minimum = 0.01, maximum = 50000 },
                    idempotencyKey = new { type = "string", format = "uuid",
                        description = "Client-generated UUID for deduplication" }
                }
            },
            XIntent = "Move funds between accounts — irreversible",
            XRiskProfile = "high",
            XReversible = false,
            XConstraints = new { max_amount = 50000, requires_mfa = true },
            XAgentGuidance = "STOP. Confirm recipient and amount with user before calling."
        }
    ],
    Prompts = [
        new McpPrompt
        {
            Name = "summarise-transactions",
            Description = "Generate a natural language summary of account transactions"
        }
    ]
}).AllowAnonymous();
```

---

## Agent Discovery Flow

```csharp
// Agent startup — discover capabilities
var manifest = await _mcpClient.GetManifestAsync();

// Index tools by name and intent
var toolsByName = manifest.Tools.ToDictionary(t => t.Name);
var toolsByIntent = manifest.Tools
    .Where(t => t.XIntent is not null)
    .GroupBy(t => t.XIntent!)
    .ToDictionary(g => g.Key, g => g.First());

// Build risk map
var riskMap = manifest.Tools.ToDictionary(
    t => t.Name,
    t => t.XRiskProfile ?? "medium");

// When user makes a request, match intent to tool
async Task HandleUserRequest(string userIntent, CancellationToken ct)
{
    var tool = FindBestTool(manifest.Tools, userIntent);
    if (tool is null) throw new InvalidOperationException("No matching tool");

    var risk = riskMap[tool.Name];

    switch (risk)
    {
        case "low":
            // Green — call freely
            await _mcpClient.CallToolAsync(tool.Name, BuildArgs(tool, userIntent), ct);
            break;

        case "medium":
            // Yellow — log and proceed
            _logger.LogInformation("Calling medium-risk tool {Tool}", tool.Name);
            await _mcpClient.CallToolAsync(tool.Name, BuildArgs(tool, userIntent), ct);
            break;

        case "high":
            // Red — pause and confirm
            var confirmed = await _userInterface.ConfirmAsync(
                $"This will {tool.XIntent}. Are you sure?");
            if (!confirmed) return;
            await _mcpClient.CallToolAsync(tool.Name, BuildArgs(tool, userIntent), ct);
            break;
    }
}
```

---

## Trust Levels

| Risk Profile | Agent Behaviour |
| :--- | :--- |
| 🟢 `low` | Read-only — call freely, no confirmation |
| 🟡 `medium` | Reversible state change — log and proceed |
| 🔴 `high` | Irreversible or financial — pause, confirm with user, include idempotency key |
| 🚫 `blocked` | Out of scope for this agent's policy — refuse and log |

---

## Versioned Manifests

New capabilities appear in the manifest without agent changes. Deprecated tools signal sunset.

```json
{
  "tools": [
    { "name": "transfer-funds", "deprecated": false },
    { "name": "legacy-wire-transfer", "deprecated": true,
      "deprecationMessage": "Use transfer-funds instead. Removing 2025-06-01.",
      "replacedBy": "transfer-funds" }
  ]
}
```

Agents that re-fetch the manifest on startup automatically:
- Discover new tools on server deploy
- Stop calling deprecated tools when `deprecated: true` appears
- Switch to replacement tool if `replacedBy` is set

---

## Discovery Failure Modes

| Missing | Consequence |
| :--- | :--- |
| `inputSchema` | Agent can't validate arguments — may send malformed requests |
| `x-risk-profile` | Agent assumes safe — may call irreversible operations without confirmation |
| Ambiguous description | Agent selects the wrong tool |
| No `deprecated: true` | Agent calls removed tools → 404/410 errors |
| No `replacedBy` | Agent can't automatically migrate to new tool |

---

## The Complete Agentic Loop

```
Discovery  → GET /mcp/manifest
              "What can you do? What's risky?"

Read       → resources/read bank://accounts/uuid
              "Show me the data before I act"

Act        → tools/call transfer-funds { ... }
              "Execute — with confirmation for high-risk"
```

---

## Further Reading

- [MCP Tool Discovery spec](https://spec.modelcontextprotocol.io/specification/server/tools/)
- [ModelContextProtocol.NET](https://github.com/modelcontextprotocol/csharp-sdk)
- [Building MCP servers in .NET](https://devblogs.microsoft.com/dotnet/build-ai-apps-with-model-context-protocol-and-dotnet/)

## Your Next Step

We've explored the future of API engineering and how agents will interact with our services. Now, it's time to get our hands dirty with the language that makes it all possible.

Dive into the core of .NET development in: **[Module 2: C# Language Basics](../../Basics/README.md)**.
