# Policy as Code in .NET

Externalise authorisation logic so rules can change without redeploying services.

---

## The Problem with Hardcoded Auth Rules

```csharp
// ❌ Rules scattered across every microservice
if (user.Role == "admin" || (user.Role == "manager" && request.Amount < 10000))
    DoTransfer();
```

Problems:
- Same rule duplicated in Account Service, Transfer Service, Notification Service
- Changing the rule requires deploying all three services simultaneously
- No audit trail — who changed the rule and when?
- Compliance teams can't audit rules without reading code

---

## Solution: Open Policy Agent (OPA)

Services send a query to OPA: "can this user perform this action on this resource?" OPA evaluates a Rego policy and returns `allow: true/false`.

```
Your Service → POST /v1/data/bank/authz/allow { user, action, resource }
OPA          → evaluates Rego policy
OPA          → returns { result: true }
Your Service → proceed with request
```

OPA runs as a sidecar or standalone service. Policies are version-controlled in Git.

---

## Rego Policy Example

```rego
# bank_policy.rego
package bank.authz

import future.keywords.if
import future.keywords.in

default allow := false

# Admins can do anything
allow if {
    "admin" in input.user.roles
}

# Account holders can read their own accounts
allow if {
    input.action == "read:account"
    input.user.id == input.resource.owner_id
}

# Managers can approve transfers up to $10,000
allow if {
    input.action == "approve:transfer"
    "manager" in input.user.roles
    input.resource.amount <= 10000
}
```

Test with `opa test`:
```bash
opa test bank_policy.rego bank_policy_test.rego -v
```

---

## ASP.NET Core + OPA Integration

```csharp
// OPA client (HTTP call to OPA sidecar)
public class OpaClient(HttpClient http)
{
    public async Task<bool> IsAllowedAsync(PolicyRequest request, CancellationToken ct)
    {
        var response = await http.PostAsJsonAsync(
            "/v1/data/bank/authz/allow",
            new { input = request }, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<OpaResult>(cancellationToken: ct);
        return result?.Result ?? false;
    }
}

public record PolicyRequest(
    PolicyUser User,
    string Action,
    string Resource,
    string? ResourceId,
    decimal? Amount);

public record PolicyUser(string Id, IEnumerable<string> Roles);
public record OpaResult(bool Result);

// Authorisation handler that delegates to OPA
public class OpaAuthorizationHandler(OpaClient opa)
    : AuthorizationHandler<OpaRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OpaRequirement requirement)
    {
        var request = new PolicyRequest(
            User: new PolicyUser(context.User.GetUserId().ToString(), context.User.GetRoles()),
            Action: requirement.Action,
            Resource: requirement.Resource,
            ResourceId: requirement.ResourceId,
            Amount: requirement.Amount);

        if (await opa.IsAllowedAsync(request, CancellationToken.None))
            context.Succeed(requirement);
    }
}
```

---

## Enforcement Levels

| Level | Where | Granularity |
| :--- | :--- | :--- |
| **API Gateway** | Edge (before your app) | Coarse — per-route |
| **Middleware** | Inside your app | Medium — per-request |
| **Handler** | Resource-level checks | Fine — per-object |

All levels can share the same Rego policy files.

---

## Policy Lifecycle

```
Developer writes/updates Rego policy
→ `opa test` runs in CI (unit tests for policies)
→ `opa fmt` enforces policy formatting
→ Policy committed to Git (audit trail)
→ OPA hot-reloads policy bundle (zero downtime)
→ All services pick up new rules without restart
```

No service deployment needed to change an authorisation rule.

---

## RBAC vs ABAC Decision Guide

```
Are decisions purely role-based? (admin, user, manager)
├── YES → RBAC — use [Authorize(Roles = "...")] or named policies
└── NO  → Do decisions depend on resource attributes or context?
          (owner, amount, time, tenant, geo)
          ├── YES → ABAC — use resource-based IAuthorizationHandler or OPA
          └── NO  → RBAC is probably sufficient
```

---

## Further Reading

- [Open Policy Agent](https://www.openpolicyagent.org/)
- [Resource-based authorization in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/resourcebased)
- [OPA .NET integration](https://www.openpolicyagent.org/docs/latest/integration/)
