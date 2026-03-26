# API Versioning in .NET

Managing multiple API versions in production so clients can migrate at their own pace.

---

## Two Strategies

### 1. Explicit Versioning (run v1 and v2 simultaneously)

Ship the new version, keep the old one alive, deprecate after clients migrate.

```
Today: only /v1/transfers exists
Ship:  /v2/transfers with new response shape
Wait:  clients migrate from v1 to v2
Remove: /v1/transfers after zero traffic
```

### 2. Extend & Contract (field migration without a new version)

Add new fields alongside old ones. Wait for clients to migrate. Remove old fields.

```
Phase 1: Both shapes accepted (add new fields, old fields still work)
Phase 2: Old fields removed after migration window closes
```

---

## URL Path Versioning (Recommended)

URL path versioning is the most explicit and widely adopted approach.

```csharp
// Program.cs — install Asp.Versioning.Http NuGet package
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

// v1 controller
[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/accounts")]
public class AccountsV1Controller(IBankService svc) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var account = await svc.GetAccountAsync(id, ct);
        return account is null ? NotFound() : Ok(new AccountV1Dto(account));
    }
}

// v2 controller — new response shape
[ApiController]
[ApiVersion("2.0")]
[Route("v{version:apiVersion}/accounts")]
public class AccountsV2Controller(IBankService svc) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var account = await svc.GetAccountAsync(id, ct);
        return account is null ? NotFound() : Ok(new AccountV2Dto(account));
    }
}
```

---

## Alternative: Header Versioning

```csharp
options.ApiVersionReader = new HeaderApiVersionReader("X-API-Version");
// Request: GET /accounts/123  X-API-Version: 2.0
```

Pros: clean URLs. Cons: harder to test with browser/curl; doesn't show up in HTTP caches.

---

## Breaking vs Non-Breaking Changes

| Change | Breaking? | Action |
| :--- | :--- | :--- |
| Remove a field from response | ✅ Breaking | New version required |
| Rename a field | ✅ Breaking | New version required |
| Remove an endpoint | ✅ Breaking | Deprecate then remove |
| Change a field type | ✅ Breaking | New version required |
| Add an optional request field | ❌ Non-breaking | Safe to ship to v1 |
| Add a new optional response field | ❌ Non-breaking | Safe to ship to v1 |
| Add a new endpoint | ❌ Non-breaking | Safe to ship |
| Add an optional query parameter | ❌ Non-breaking | Safe to ship |

---

## Adapter Pattern: One Domain Model, Multiple Wire Shapes

Business logic should never know about versioning. Use adapters at the boundary.

```csharp
// Domain model — no version concern
public class Account { public Guid Id; public string Owner; public decimal Balance; }

// v1 wire shape — flat, simple
public record AccountV1Dto(Guid Id, string Owner, decimal Balance)
{
    public AccountV1Dto(Account a) : this(a.Id, a.Owner, a.Balance) { }
}

// v2 wire shape — adds audit fields
public record AccountV2Dto(Guid Id, string Owner, decimal Balance,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt)
{
    public AccountV2Dto(Account a) : this(a.Id, a.Owner, a.Balance, a.CreatedAt, a.UpdatedAt) { }
}
```

---

## Version Lifecycle

```
Beta       → early access, may break at any time
GA         → stable, semver guarantees apply
Deprecated → still works; Deprecation + Sunset headers injected by middleware
Sunset     → returns 410 Gone with migration guide URL
Retired    → code removed, instances terminated
```

---

## Extend & Contract — Dual Adapter Example

```csharp
// Phase 1: accept both old and new field names
public class CreateTransferRequest
{
    // New name
    public Guid? FromAccountId { get; init; }

    // Old name — deprecated; remove after migration window
    [Obsolete("Use FromAccountId")]
    public Guid? SourceAccountId { get; init; }

    // Resolve whichever was provided
    public Guid ResolvedFromId =>
        FromAccountId ?? SourceAccountId
        ?? throw new ArgumentException("FromAccountId is required");
}
```

---

## Further Reading

- [Asp.Versioning.Http NuGet package](https://github.com/dotnet/aspnet-api-versioning)
- [API versioning in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/web-api/advanced/conventions)
