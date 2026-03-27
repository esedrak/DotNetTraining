# Cloud Deployment for .NET APIs

Production-ready deployment patterns for ASP.NET Core on cloud platforms.

---

## Health Check Endpoints

Two separate endpoints serving different consumers:

```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddNpgsql(connectionString, name: "database")
    .AddCheck("self", () => HealthCheckResult.Healthy());

// /healthz — liveness: "is the process running?"
// ECS / Kubernetes restarts the container if this returns non-200
app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    Predicate = check => check.Name == "self",
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// /readyz — readiness: "is the app ready to serve traffic?"
// Load balancer removes the instance from rotation if this returns non-200
app.MapHealthChecks("/readyz", new HealthCheckOptions
{
    Predicate = _ => true,  // includes DB check
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

| Endpoint | Consumer | Failure action |
| :--- | :--- | :--- |
| `/healthz` | ECS / Kubernetes | Restart the container |
| `/readyz` | ALB / Ingress | Remove from load balancer rotation |

---

## Graceful Shutdown

When ECS/Kubernetes sends `SIGTERM`, the app must:
1. Stop accepting new connections
2. Drain in-flight requests
3. Exit cleanly

```csharp
// ASP.NET Core handles this automatically — but you must set shutdown timeout
builder.Services.Configure<HostOptions>(opts =>
{
    opts.ShutdownTimeout = TimeSpan.FromSeconds(30);
});

// For custom cleanup (close DB connections, flush queues, etc.)
builder.Services.AddHostedService<ShutdownCleanupService>();

public class ShutdownCleanupService(ILogger<ShutdownCleanupService> logger)
    : IHostedService
{
    public Task StartAsync(CancellationToken ct) => Task.CompletedTask;

    public async Task StopAsync(CancellationToken ct)
    {
        logger.LogInformation("Graceful shutdown initiated — draining in-flight requests");
        await Task.Delay(TimeSpan.FromSeconds(5), ct); // wait for load balancer drain
    }
}
```

---

## Rolling Deployment Sequence (ECS)

```
1. New task starts (image 1.4.2)
2. /readyz returns 503 → startup probe, DB migration running
3. ALB keeps 100% traffic on old tasks (image 1.4.1)
4. DB migration complete → /readyz returns 200
5. ALB adds new task to rotation
6. ECS sends SIGTERM to one old task
7. Old task's /readyz returns 503 immediately (drain signal)
8. ALB stops sending new requests to old task
9. Old task finishes in-flight requests (30s drain window)
10. Old task exits 0
11. Repeat for remaining old tasks
```

---

## /readyz during Shutdown

The readiness endpoint must return 503 the moment SIGTERM is received:

```csharp
public class ReadinessMiddleware(RequestDelegate next) : IMiddleware
{
    private static volatile bool _isReady = false;

    public static void MarkReady() => _isReady = true;
    public static void MarkNotReady() => _isReady = false;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Path == "/readyz" && !_isReady)
        {
            context.Response.StatusCode = 503;
            return;
        }
        await next(context);
    }
}

// In Program.cs — register lifecycle hooks
var lifetime = app.Lifetime;
lifetime.ApplicationStarted.Register(ReadinessMiddleware.MarkReady);
lifetime.ApplicationStopping.Register(ReadinessMiddleware.MarkNotReady);
```

---

## Blue/Green vs Rolling Deployment

| Strategy | Cost | Rollback | In-flight version mixing |
| :--- | :--- | :--- | :--- |
| **Rolling** | No extra infra | Slow (re-deploy old version) | Brief window with v1 + v2 in parallel |
| **Blue/Green** | 2× infra during cutover | Instant (flip LB back) | Never — clean cutover |

Rolling is the default in ECS and Kubernetes — no extra cost and sufficient for most workloads. Use blue/green when you need instant rollback for financial or regulated APIs.

---

## ECS Task Definition Best Practices

```json
{
  "image": "myregistry.azurecr.io/bank-api:1.4.2",
  "cpu": 256,
  "memory": 512,
  "healthCheck": {
    "command": ["CMD-SHELL", "curl -f http://localhost:8080/healthz || exit 1"],
    "interval": 30,
    "timeout": 5,
    "retries": 3,
    "startPeriod": 60
  },
  "environment": [
    { "name": "ASPNETCORE_ENVIRONMENT", "value": "Production" }
  ],
  "secrets": [
    { "name": "ConnectionStrings__Default", "valueFrom": "arn:aws:secretsmanager:..." }
  ],
  "logConfiguration": {
    "logDriver": "awslogs",
    "options": {
      "awslogs-group": "/ecs/bank-api",
      "awslogs-region": "ap-southeast-2",
      "awslogs-stream-prefix": "ecs"
    }
  }
}
```

Key rules:
- **Immutable image tag** — never `latest` in production
- **Secrets from AWS Secrets Manager** — never baked into the image or task definition environment
- **Right-size CPU/memory** — monitor P99 usage and set limits at ~2× P99

---

## Kubernetes Equivalent

```yaml
# deployment.yaml
spec:
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxUnavailable: 0
      maxSurge: 1
  template:
    spec:
      containers:
        - image: myregistry/bank-api:1.4.2
          livenessProbe:
            httpGet: { path: /healthz, port: 8080 }
            initialDelaySeconds: 10
            periodSeconds: 30
          readinessProbe:
            httpGet: { path: /readyz, port: 8080 }
            initialDelaySeconds: 5
            periodSeconds: 10
          lifecycle:
            preStop:
              exec:
                command: ["/bin/sh", "-c", "sleep 10"]  # drain window
```

---

## Further Reading

- [ASP.NET Core health checks](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
- [Graceful shutdown in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host#host-shutdown)
- [.NET on AWS ECS](https://aws.amazon.com/getting-started/hands-on/deploy-dotnet-web-app-ecs-fargate/)

## Your Next Step

We've mastered the lifecycle of human-consumed APIs, but a new era is upon us. How do we design our APIs for the next generation of consumers: AI agents?

Explore the future of API engineering in: **[Designing APIs for AI Consumption](../TheAgenticFuture/01-designing-apis-for-ai.md)**.
