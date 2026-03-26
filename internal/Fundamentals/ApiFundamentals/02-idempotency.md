# Idempotency in APIs

Calling the same operation N times produces the same result as calling it once.

---

## Why It Matters

Network calls fail. Clients retry. Without idempotency, retries cause duplicate state:

```
Client → POST /transfers { from: A, to: B, amount: 100 }
Server: deducted $100 from A, added $100 to B
Network: response lost (timeout)
Client: retries...
Client → POST /transfers { from: A, to: B, amount: 100 }
Server: deducted $100 from A AGAIN → Alice lost $200 for one intended transfer
```

---

## HTTP Methods and Idempotency

| Method | Idempotent? | Notes |
| :--- | :--- | :--- |
| `GET` | ✅ | Safe — no state change |
| `PUT` | ✅ | Replaces the full resource; same result every time |
| `DELETE` | ✅ | Return 204 even if already deleted |
| `POST` | ❌ | Creates new resource each time — use `Idempotency-Key` |
| `PATCH` | ❌* | Idempotent only if setting absolute values, not increments |

---

## The Fix: Idempotency-Key Header

Client generates a UUID before the first attempt and sends it on every retry:

```
POST /v1/transfers HTTP/1.1
Content-Type: application/json
Idempotency-Key: 550e8400-e29b-41d4-a716-446655440000

{ "fromAccountId": "...", "toAccountId": "...", "amount": 100.00 }
```

Server stores the key → result mapping. On retry, the cached result is returned without re-executing.

---

## ASP.NET Core Implementation

```csharp
// Idempotency middleware
public class IdempotencyMiddleware(RequestDelegate next, IDistributedCache cache)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Only guard state-changing methods
        if (context.Request.Method is "GET" or "HEAD")
        {
            await next(context);
            return;
        }

        var key = context.Request.Headers["Idempotency-Key"].FirstOrDefault();
        if (key is null)
        {
            await next(context);
            return;
        }

        var cacheKey = $"idempotency:{key}";
        var cached = await cache.GetStringAsync(cacheKey);
        if (cached is not null)
        {
            // Return cached response — no side effects executed
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(cached);
            return;
        }

        // Capture the response body
        var originalBody = context.Response.Body;
        using var buffer = new MemoryStream();
        context.Response.Body = buffer;
        await next(context);

        // Cache for 24 hours
        buffer.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(buffer).ReadToEndAsync();
        await cache.SetStringAsync(cacheKey, responseBody,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) });

        buffer.Seek(0, SeekOrigin.Begin);
        context.Response.Body = originalBody;
        await buffer.CopyToAsync(originalBody);
    }
}
```

Register:
```csharp
app.UseMiddleware<IdempotencyMiddleware>();
```

---

## Service Layer Pattern

```csharp
public class TransferService(IBankRepository repo, IDistributedCache cache)
{
    public async Task<Transfer> CreateTransferAsync(
        CreateTransferCommand cmd, CancellationToken ct)
    {
        if (cmd.IdempotencyKey is not null)
        {
            var existing = await repo.FindTransferByIdempotencyKeyAsync(cmd.IdempotencyKey, ct);
            if (existing is not null)
                return existing; // Return cached result, no double charge
        }

        // ... execute transfer
        var transfer = new Transfer
        {
            IdempotencyKey = cmd.IdempotencyKey,
            // ...
        };
        await repo.SaveTransferAsync(transfer, ct);
        return transfer;
    }
}
```

---

## Idempotency Key Lifecycle

1. **Generate**: client creates `Guid.NewGuid()` before the first attempt
2. **Store**: server stores `key → response` in Redis or PostgreSQL
3. **Expire**: keys expire after 24h (balance between safety and storage)
4. **Conflict**: if the same key arrives with different parameters → return `422` or `409`

---

## DELETE Idempotency

Always return `204 No Content` even if already deleted:

```csharp
[HttpDelete("{id:guid}")]
public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
{
    await _svc.DeleteAsync(id, ct); // no-op if already deleted
    return NoContent(); // ✅ 204 always, not 404 on second call
}
```

---

## Financial API Rule

> **All payment, transfer, and notification endpoints MUST support `Idempotency-Key`.**

This is non-negotiable. Network timeouts are guaranteed at scale. Without idempotency, every retry is a potential double-charge.

---

## Further Reading

- [Stripe idempotency guide](https://stripe.com/docs/idempotency)
- [IDistributedCache in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed)
