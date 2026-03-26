# Notable API Security Incidents

Real-world case studies demonstrating the consequences of poor API design decisions. These incidents are studied to learn what NOT to do.

---

## 1. Optus Data Breach (2022)

**Vulnerability**: BOLA (Broken Object Level Authorisation) + Missing Authentication  
**Impact**: 9.8 million Australians' PII exposed (names, DOBs, phone numbers, passport/driver's licence numbers)

### What Happened

An unauthenticated API endpoint accepted sequential integer user IDs:

```
GET /api/v1/customers/1001
GET /api/v1/customers/1002
GET /api/v1/customers/1003
```

No authentication was required. No rate limiting was applied. An attacker automated calls through the entire ID space in hours.

### Violations

| OWASP API Top 10 | Violation |
| :--- | :--- |
| API1: BOLA | Sequential integer IDs — any authenticated user could access any other user's record |
| API2: Authentication | Endpoint was publicly accessible without any token or credential |
| API4: Unrestricted Resource Consumption | No rate limiting — millions of records scraped in hours |
| API3: Excessive Data Exposure | Full PII returned including government ID numbers not needed by consumers |

### The Fix in ASP.NET Core

```csharp
// ✅ Always use [Authorize] and UUIDs
[HttpGet("{id:guid}")]
[Authorize]
public async Task<IActionResult> GetCustomer(Guid id, CancellationToken ct)
{
    var customer = await _repo.GetByIdAsync(id, ct);
    if (customer is null) return NotFound();

    // ✅ Verify caller owns this resource
    var callerId = User.GetUserId();
    if (customer.UserId != callerId && !User.IsInRole("admin"))
        return Forbid();

    // ✅ Return only what the caller needs
    return Ok(new CustomerSummaryDto(customer.Name, customer.Phone));
}
```

Add rate limiting:
```csharp
builder.Services.AddRateLimiter(options =>
    options.AddSlidingWindowLimiter("api", o =>
    {
        o.PermitLimit = 100;
        o.Window = TimeSpan.FromMinutes(1);
        o.SegmentsPerWindow = 6;
    }));
```

---

## 2. Coinbase API Vulnerability (2022)

**Vulnerability**: Broken Function Level Authorization + Insufficient Input Validation  
**Impact**: Users could trade assets they did not own

### What Happened

A flaw in the order execution API allowed a user to submit a trade referencing another user's account. The balance check and the trade execution were not atomic — a race condition between the check and the trade allowed the invariant to be violated.

### Violations

| OWASP API Top 10 | Violation |
| :--- | :--- |
| API5: Function Level Authorization | Trade endpoint didn't verify the source account belonged to the authenticated user |
| API8: Security Misconfiguration | Non-atomic check-then-act on financial balances |

### The Fix

```csharp
// ✅ Single atomic DB transaction — EF Core
public async Task<Transfer> CreateTransferAsync(
    Guid fromAccountId, Guid toAccountId, decimal amount,
    Guid callerId, CancellationToken ct)
{
    await using var transaction = await _db.Database.BeginTransactionAsync(ct);
    try
    {
        var from = await _db.Accounts
            .Where(a => a.Id == fromAccountId && a.OwnerId == callerId) // ✅ ownership check
            .FirstOrDefaultAsync(ct)
            ?? throw new ForbiddenException("Account does not belong to caller");

        var to = await _db.Accounts.FindAsync([toAccountId], ct)
            ?? throw new NotFoundException("Destination account not found");

        from.Withdraw(amount);   // throws InsufficientFundsException if balance too low
        to.Deposit(amount);

        var transfer = new Transfer { FromAccountId = fromAccountId, ToAccountId = toAccountId, Amount = amount };
        _db.Transfers.Add(transfer);
        await _db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return transfer;
    }
    catch
    {
        await transaction.RollbackAsync(ct);
        throw;
    }
}
```

---

## 3. First American Financial Document API (2019)

**Vulnerability**: BOLA/IDOR  
**Impact**: 885 million sensitive financial documents exposed (bank records, SSNs, mortgage documents)

### What Happened

Document IDs were sequential integers in query parameters:

```
GET /api/docs?id=000000001
GET /api/docs?id=000000002
```

No authorization checks were performed — any authenticated user could retrieve any document by incrementing the ID.

### Violations

| OWASP API Top 10 | Violation |
| :--- | :--- |
| API1: BOLA | Predictable sequential IDs with no ownership check |
| API2: Authentication | Authentication existed but authorisation was missing |
| API3: Excessive Data Exposure | Full financial documents returned including SSNs and bank account numbers |

### The Fix

```csharp
// ✅ Use UUIDs — not sequential integers
public class Document
{
    public Guid Id { get; init; } = Guid.NewGuid(); // unpredictable
    public required Guid OwnerId { get; init; }
}

// ✅ Always check ownership
[HttpGet("{id:guid}")]
[Authorize]
public async Task<IActionResult> GetDocument(Guid id, CancellationToken ct)
{
    var doc = await _repo.GetDocumentAsync(id, ct);
    if (doc is null) return NotFound();

    var callerId = User.GetUserId();
    if (doc.OwnerId != callerId)
        return NotFound(); // ✅ 404 not 403 — don't reveal existence
    
    // ✅ Return minimum necessary fields
    return Ok(new DocumentSummaryDto(doc.Title, doc.CreatedAt));
}
```

---

## Common Themes

All three incidents share the same root causes:

| Root Cause | Mitigation |
| :--- | :--- |
| **Sequential / predictable IDs** | Always use `Guid.NewGuid()` for resource IDs |
| **Missing ownership checks** | Verify `resource.OwnerId == caller.Id` on every access |
| **No rate limiting** | Apply `Microsoft.AspNetCore.RateLimiting` globally |
| **Excessive data exposure** | Return DTOs with minimum necessary fields — never return domain entities directly |
| **Missing anomaly detection** | Add structured logging + alerting on unusual access patterns |

### OWASP API Security Top 10 (2023)

1. **API1**: Broken Object Level Authorisation (BOLA/IDOR)
2. **API2**: Broken Authentication
3. **API3**: Broken Object Property Level Authorisation
4. **API4**: Unrestricted Resource Consumption
5. **API5**: Broken Function Level Authorisation
6. **API6**: Unrestricted Access to Sensitive Business Flows
7. **API7**: Server Side Request Forgery (SSRF)
8. **API8**: Security Misconfiguration
9. **API9**: Improper Inventory Management
10. **API10**: Unsafe Consumption of APIs
