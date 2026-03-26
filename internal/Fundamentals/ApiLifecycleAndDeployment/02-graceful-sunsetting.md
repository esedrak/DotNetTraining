# Graceful API Sunsetting

Deprecation is a **process** (announce → grace period → sunset → remove), not an event.

---

## Why It Matters

Removing a version without warning breaks clients. Clients may not notice deprecation headers unless you also contact them directly. The standard process:

1. **Announce** — blog post, email, changelog
2. **Deprecation headers** — every response on deprecated endpoints includes headers
3. **Grace period** — minimum 6 months for external APIs, 4 weeks for internal
4. **Sunset date** — announced in headers and documentation
5. **410 Gone** — after sunset date, return 410 with migration link
6. **Remove** — code removed after traffic confirms zero usage

---

## Standard Deprecation Headers

RFC 8594 defines `Sunset`. The `Deprecation` header is a draft standard widely adopted by API providers.

```
Deprecation: true
Sunset: Sat, 01 Jun 2025 00:00:00 GMT
Link: <https://docs.example.com/migrate/v1-to-v2>; rel="sunset"
```

---

## ASP.NET Core Deprecation Middleware

```csharp
// Thin decorator — the handler is completely unaware it's deprecated
public class DeprecationMiddleware(
    RequestDelegate next,
    string sunsetDate,
    string migrationUrl)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Inject headers on every request to deprecated endpoints
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["Deprecation"] = "true";
            context.Response.Headers["Sunset"] = sunsetDate;
            context.Response.Headers["Link"] = $"<{migrationUrl}>; rel=\"sunset\"";
            return Task.CompletedTask;
        });

        await next(context);
    }
}

// Register only on v1 routes
app.MapGroup("v1").AddEndpointFilter<DeprecationFilter>()
    .RequireAuthorization();
// or wrap just the v1 controllers
app.UseWhen(
    ctx => ctx.Request.Path.StartsWithSegments("/v1"),
    branch => branch.UseMiddleware<DeprecationMiddleware>(
        "Sat, 01 Jun 2025 00:00:00 GMT",
        "https://docs.example.com/migrate/v1-to-v2"));
```

---

## After Sunset: 410 Gone

Return `410 Gone` (not `404 Not Found`) after the sunset date. 410 is **permanent** — it tells clients and caches that this resource is gone forever.

```csharp
// Minimal API — redirect all v1 routes to 410
app.MapGroup("v1/{**catch-all}").MapGet("/", (HttpContext ctx) =>
{
    ctx.Response.Headers["Link"] =
        "<https://docs.example.com/migrate/v1-to-v2>; rel=\"sunset\"";
    return Results.Problem(
        title: "API version retired",
        detail: "v1 was retired on 2025-06-01. See the Link header for the migration guide.",
        statusCode: 410);
});
```

---

## Tracking Migration Progress

Never remove a version without checking that traffic has reached zero.

```csharp
// Log a metric for every v1 request
public class VersionTrackingMiddleware(RequestDelegate next, ILogger<VersionTrackingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var version = context.GetRequestedApiVersion()?.ToString() ?? "unknown";

        // Structured log — queryable in your log aggregator
        logger.LogInformation("ApiRequest {Version} {Method} {Path} {UserAgent}",
            version,
            context.Request.Method,
            context.Request.Path,
            context.Request.Headers.UserAgent.ToString());

        await next(context);
    }
}
```

Query in your log aggregator:
```sql
-- Find who is still calling v1
SELECT user_agent, api_key, COUNT(*) as calls
FROM api_logs
WHERE version = 'v1'
  AND timestamp > NOW() - INTERVAL '7 days'
GROUP BY user_agent, api_key
ORDER BY calls DESC;
```

---

## Deprecation Checklist

- [ ] Announce deprecation date publicly (blog/email/changelog)
- [ ] Add `Deprecation`, `Sunset`, and `Link` headers to all v1 responses
- [ ] Monitor v1 traffic — alert when it exceeds threshold
- [ ] Contact API consumers still sending requests to v1
- [ ] On sunset date: switch to 410 Gone
- [ ] After 30 days of zero traffic: remove v1 code

---

## Further Reading

- [RFC 8594 — Sunset Header](https://www.rfc-editor.org/rfc/rfc8594)
- [Asp.Versioning deprecation](https://github.com/dotnet/aspnet-api-versioning/wiki/Deprecating-an-API-Version)
