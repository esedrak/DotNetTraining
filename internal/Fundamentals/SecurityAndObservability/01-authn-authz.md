# Authentication and Authorisation in ASP.NET Core

**AuthN** (Authentication) — Who are you?  
**AuthZ** (Authorisation) — What are you allowed to do?

AuthN must succeed before AuthZ is evaluated.

---

## JWT (JSON Web Token)

The standard for stateless API authentication.

### Structure
```
eyJhbGc...   ←  Header (algorithm: HS256 or RS256)
.
eyJzdWIi...  ←  Payload (claims: sub, roles, exp, jti)
.
SflKxw...    ←  Signature (HMAC-SHA256 of header.payload)
```

Decoded payload:
```json
{
  "sub": "user-uuid-here",
  "name": "Alice Smith",
  "roles": ["account-holder", "admin"],
  "exp": 1735689600,
  "jti": "unique-token-id"
}
```

### ASP.NET Core JWT Setup

```csharp
// Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"];
        options.Audience = builder.Configuration["Auth:Audience"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

app.UseAuthentication(); // must come before UseAuthorization
app.UseAuthorization();

// Controllers — require auth by default
[ApiController]
[Authorize]  // ← all endpoints require a valid JWT
public class AccountsController : ControllerBase { ... }

// Whitelist public endpoints explicitly
[HttpGet("health")]
[AllowAnonymous]
public IActionResult Health() => Ok("healthy");
```

### Typed Context Keys Pattern

Access claims in a type-safe way via a helper (avoids string literals scattered throughout the codebase):

```csharp
public static class UserContext
{
    public static Guid GetUserId(this ClaimsPrincipal user)
        => Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("User ID claim missing"));

    public static IEnumerable<string> GetRoles(this ClaimsPrincipal user)
        => user.FindAll(ClaimTypes.Role).Select(c => c.Value);

    public static bool IsAdmin(this ClaimsPrincipal user)
        => user.IsInRole("admin");
}

// Usage in controllers
var userId = User.GetUserId();
var isAdmin = User.IsAdmin();
```

---

## OAuth 2.0 Delegated Access

For third-party apps that need to act on behalf of users.

```
User → authorises ThirdPartyApp on your Auth Server
Auth Server → issues access token to ThirdPartyApp
ThirdPartyApp → calls Bank API with Bearer token
Bank API → validates token signature (trusts Auth Server's JWKS)
```

Your Bank API only needs the auth server's public key (from the JWKS endpoint) — it never sees the user's password.

---

## RBAC (Role-Based Access Control)

Simple, coarse-grained: roles have permissions.

```csharp
// Named policy — declarative and testable
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanTransfer", policy =>
        policy.RequireRole("account-holder", "admin"));

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("admin"));

    options.AddPolicy("ReadOnly", policy =>
        policy.RequireClaim("scope", "read:accounts"));
});

// Apply at endpoint level
[HttpPost]
[Authorize(Policy = "CanTransfer")]
public async Task<IActionResult> CreateTransfer(...) { }
```

## ABAC (Attribute-Based Access Control)

Fine-grained: decisions based on resource attributes, user attributes, and context.

```csharp
// Custom requirement
public class AccountOwnerRequirement : IAuthorizationRequirement { }

// Handler checks resource-level ownership
public class AccountOwnerHandler(IBankRepository repo)
    : AuthorizationHandler<AccountOwnerRequirement, Account>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AccountOwnerRequirement requirement,
        Account account)
    {
        var userId = context.User.GetUserId();
        if (account.OwnerId == userId || context.User.IsAdmin())
            context.Succeed(requirement);
    }
}

// Register
builder.Services.AddSingleton<IAuthorizationHandler, AccountOwnerHandler>();
```

---

## HTTP Auth Status Codes

| Code | Meaning | When to Use |
| :--- | :--- | :--- |
| `401 Unauthorized` | Identity problem — re-authenticate | Invalid/expired/missing token |
| `403 Forbidden` | Permission denied — authenticated but not authorised | Valid token, wrong role/policy |
| `404 Not Found` | Resource doesn't exist or caller can't see it | Use instead of 403 when you want to hide resource existence |

> **Security tip**: When checking resource ownership, return `404` not `403` if the caller doesn't own the resource. Returning `403` reveals the resource exists.

---

## Token Revocation

JWTs are stateless — a compromised token is valid until expiry. Maintain a Redis revocation list.

```csharp
// On logout or compromise detection
public async Task RevokeTokenAsync(ClaimsPrincipal user)
{
    var jti = user.FindFirstValue(JwtRegisteredClaimNames.Jti)!;
    var expiry = user.FindFirstValue(JwtRegisteredClaimNames.Exp)!;
    var ttl = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiry)) - DateTimeOffset.UtcNow;

    await _cache.SetStringAsync($"revoked:{jti}", "1",
        new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl });
}

// Check in middleware or policy handler
var jti = context.User.FindFirstValue(JwtRegisteredClaimNames.Jti);
if (await _cache.GetStringAsync($"revoked:{jti}") is not null)
    return Results.Unauthorized();
```

---

## Further Reading

- [JWT Bearer authentication in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)
- [Authorization in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/introduction)
- [Resource-based authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/resourcebased)

## Your Next Step

Understanding identity and permissions is critical, but how do we manage those permissions at scale without cluttering our application code with complex `if/else` logic?

Explore how to externalise your authorisation rules in: **[Policy as Code](02-policy-as-code.md)**.
