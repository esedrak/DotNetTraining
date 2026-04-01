# API Design Principles

A comprehensive reference for designing APIs with ASP.NET Core. These principles apply regardless of framework, but examples use .NET idioms.

---

## Core API Styles

| Style | Transport | Format | Best For |
| :--- | :--- | :--- | :--- |
| **REST** | HTTP/1.1 + HTTP/2 | JSON | Public APIs, browser clients, human-readable contracts |
| **gRPC** | HTTP/2 | Protobuf (binary) | Internal microservices, streaming, high-throughput |
| **GraphQL** | HTTP | JSON | Flexible queries, mobile clients with bandwidth constraints |
| **SignalR** | WebSocket / SSE / Long-polling | JSON / MessagePack | Real-time push (notifications, live dashboards) |

---

## Key Design Principles

### 1. Consistency
- Use the same naming conventions, status codes, and error shapes across all endpoints.
- `.NET` convention: `camelCase` JSON properties (configure via `JsonSerializerOptions`).
- Endpoint convention: `/v1/resources/{id}` — plural nouns, kebab-case paths.

### 2. Simplicity
- One endpoint does one thing.
- Prefer `GET /accounts/{id}/transactions` over `POST /accounts/query`.
- Avoid leaking internal domain concepts (database column names, EF navigation property names).

### 3. Security by Default
- All endpoints require authentication unless explicitly public.
- Use `[Authorize]` as the default; whitelist with `[AllowAnonymous]`.
- Never return stack traces in production responses.

### 4. Performance
- Paginate large collections (`?page=1&pageSize=20`).
- Use `CancellationToken` on every async controller action.
- Add `ETag` / `Cache-Control` headers for idempotent read endpoints.

---

## Protocol Selection

```
Is this an internal microservice-to-microservice call?
├── YES → Does it require streaming or sub-millisecond latency?
│         ├── YES → gRPC (HTTP/2 + Protobuf)
│         └── NO  → REST or gRPC (both fine internally)
└── NO  → Is it consumed by browsers or external partners?
          ├── YES → REST (JSON + OpenAPI spec)
          └── NO  → REST (default unless you have a specific reason)
```

---

## HTTP Methods

| Method | Idempotent? | Safe? | Use For |
| :--- | :--- | :--- | :--- |
| `GET` | ✅ | ✅ | Retrieve a resource or collection |
| `POST` | ❌ | ❌ | Create a new resource |
| `PUT` | ✅ | ❌ | Replace a resource entirely |
| `PATCH` | ❌* | ❌ | Partially update a resource |
| `DELETE` | ✅ | ❌ | Remove a resource |
| `HEAD` | ✅ | ✅ | Check resource exists (no body) |
| `OPTIONS` | ✅ | ✅ | CORS preflight |

*`PATCH` is idempotent only if it sets absolute values, not relative increments.

---

## HTTP Status Codes

| Range | Meaning | Common Codes |
| :--- | :--- | :--- |
| `2xx` | Success | 200 OK, 201 Created, 204 No Content |
| `3xx` | Redirect | 301 Moved Permanently, 304 Not Modified |
| `4xx` | Client error | 400 Bad Request, 401 Unauthorized, 403 Forbidden, 404 Not Found, 409 Conflict, 422 Unprocessable Entity, 429 Too Many Requests |
| `5xx` | Server error | 500 Internal Server Error, 503 Service Unavailable |

---

## Resource Modelling

### Flat resources (preferred)
```
GET  /accounts          → list
POST /accounts          → create
GET  /accounts/{id}     → get by ID
PUT  /accounts/{id}     → replace
DELETE /accounts/{id}   → delete
GET  /accounts/{id}/transactions  → nested collection
```

### Controller example (ASP.NET Core)
```csharp
[ApiController]
[Route("v1/[controller]")]
public class AccountsController(IBankService svc) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
        => Ok(await svc.ListAccountsAsync(ct));

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

---

## Authentication Methods

| Method | Use Case | .NET Package |
| :--- | :--- | :--- |
| **Bearer JWT** | Stateless API auth | `Microsoft.AspNetCore.Authentication.JwtBearer` |
| **API Key** | Service-to-service | Custom middleware or `IAuthorizationHandler` |
| **OAuth 2.0** | Delegated user access | `Microsoft.AspNetCore.Authentication.OAuth` |
| **mTLS** | Zero-trust internal | Kestrel TLS + cert validation middleware |

---

## Authorisation Models

| Model | Description | .NET Implementation |
| :--- | :--- | :--- |
| **RBAC** | Role-Based — user has roles, roles have permissions | `[Authorize(Roles = "admin")]` or `IAuthorizationRequirement` |
| **ABAC** | Attribute-Based — contextual decisions (time, location, resource owner) | Custom `IAuthorizationHandler` querying resource attributes |
| **Policy** | Named policies composed of requirements | `builder.Services.AddAuthorization(opts => opts.AddPolicy(...))` |

---

## Security Checklist

- [ ] All endpoints require `[Authorize]` by default
- [ ] Input validated with `[ApiController]` + FluentValidation or DataAnnotations
- [ ] Rate limiting via `Microsoft.AspNetCore.RateLimiting` (sliding window or token bucket)
- [ ] CORS configured explicitly (`builder.Services.AddCors(...)`)
- [ ] HTTPS enforced (`app.UseHttpsRedirection()`)
- [ ] No stack traces in production (`app.UseExceptionHandler(...)`)
- [ ] Secrets from environment variables or Azure Key Vault — never in source code
- [ ] SQL injection impossible (EF Core parameterised queries or Dapper with `@param`)

---

## API Lifecycle

```
Design → Build → Test → Document → Publish → Monitor → Version → Deprecate → Sunset → Retire
```

In .NET:
- **Design**: OpenAPI YAML (contract-first) or `[ProducesResponseType]` + XML comments (code-first)
- **Document**: `app.MapOpenApi()` (spec) + Scalar (UI) on .NET 9+; Swashbuckle on .NET 8 and earlier
- **Monitor**: Serilog + OpenTelemetry + Prometheus (`prometheus-net.AspNetCore`)
- **Version**: URL path versioning (`/v1/`, `/v2/`) via `Asp.Versioning.Http`
- **Deprecate**: `Deprecation` + `Sunset` response headers via middleware

## Your Next Step

API design principles are the foundation, but what happens when they are ignored?

Explore the real-world consequences of poor design in: **[Notable API Incidents](notable-api-incidents.md)**.
