# Statelessness in APIs

Every request must carry all the information the server needs. The server remembers nothing between calls.

---

## What Statelessness Means

Each HTTP request is self-contained:

| Component | Carries |
| :--- | :--- |
| **JWT** | Identity (who you are + roles) |
| **URL + Method** | Intent (what you want) |
| **Request body** | Data (the payload) |
| **Headers** | Context (content type, idempotency key, correlation ID) |

The server processes the request, returns a response, and forgets everything.

---

## Stateful vs Stateless Authentication

### Stateful (session-based)
```
Client → POST /login
Server → stores session in memory/Redis, returns session cookie
Client → GET /accounts (cookie: session-id=abc123)
Server → looks up session in store → processes request
```

Problems:
- Sticky load balancing required (or shared session store)
- Session store becomes a bottleneck and single point of failure
- Each request requires a session lookup (extra DB/Redis hop)

### Stateless (JWT-based)
```
Client → POST /auth/token (username + password)
Server → returns JWT (signed, self-contained, expires in 1h)
Client → GET /accounts (Authorization: Bearer eyJhb...)
Server → validates JWT signature locally (no DB lookup) → processes request
```

Benefits:
- Any server instance handles any request — true horizontal scaling
- No shared state between instances
- JWT validation is CPU-only (no DB round-trip)

---

## JWT as a Stateless Identity Token

```csharp
// ASP.NET Core JWT validation — one-time setup in Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://your-auth-server.com";
        options.Audience = "bank-api";
        // Signature validated locally using authority's JWKS endpoint
        // No session lookup, no DB hit per request
    });

// Usage in a controller — claims are available after [Authorize] validates the token
[HttpGet("{id:guid}")]
[Authorize]
public async Task<IActionResult> Get(Guid id, CancellationToken ct)
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);
    // ...
}
```

---

## Scaling Advantage

```
Without state:
Load Balancer → Instance 1  ← any request can go anywhere
            → Instance 2
            → Instance 3  ← scale out by adding instances

With sessions:
Load Balancer → Instance 1  ← user A MUST go here (sticky)
            → Instance 2  ← user B MUST go here (sticky)
            → Instance 3  ← user C MUST go here (sticky)
```

Stateless APIs support **round-robin load balancing** — no affinity required.

---

## Where State Lives

| State | Where It Lives |
| :--- | :--- |
| User identity | Inside the JWT (claims) |
| Business data | PostgreSQL / SQL Server (via EF Core) |
| Session / cart | Redis (if needed) |
| File uploads | Azure Blob / S3 |
| Server instance memory | **NEVER** (violates statelessness) |

---

## Token Revocation Edge Case

JWTs are stateless — a stolen token is valid until expiry. Solution: Redis revocation list.

```csharp
public class JwtRevocationMiddleware(RequestDelegate next, IDistributedCache cache)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var jti = context.User.FindFirstValue(JwtRegisteredClaimNames.Jti);
        if (jti is not null)
        {
            var revoked = await cache.GetStringAsync($"revoked:{jti}");
            if (revoked is not null)
            {
                context.Response.StatusCode = 401;
                return;
            }
        }
        await next(context);
    }
}
```

To revoke: `cache.SetStringAsync($"revoked:{jti}", "1", expiry: tokenExpiry)`.

---

## Cache-Control

Stateless GET responses are cacheable. Mutations are not.

```csharp
// Cacheable — idempotent reads
[HttpGet("{id:guid}")]
[ResponseCache(Duration = 60, VaryByHeader = "Authorization")]
public Task<IActionResult> Get(Guid id, CancellationToken ct) { ... }

// Not cacheable — mutations
[HttpPost]
[ResponseCache(NoStore = true)]
public Task<IActionResult> Create(CreateAccountRequest req, CancellationToken ct) { ... }
```

---

## Further Reading

- [JWT in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)
- [IDistributedCache](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed)
- [Output Caching middleware](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/output)

## Your Next Step

Statelessness allows us to scale our implementation, but how do we coordinate that implementation between different teams?

Explore the different strategies for defining API contracts in: **[Contract-First vs. Code-First](04-contract-first-vs-code-first.md)**.
