# Distributed Tracing and Observability in .NET

The three pillars of observability: **Logs** (what happened), **Metrics** (how much / how fast), **Traces** (where time was spent across services).

---

## Anatomy of a Trace

A single user request spans multiple services. A trace ties them together.

```
POST /v1/transfers  210ms total
├── JWT validation          2ms
├── BankService.CreateTransfer  205ms
│   ├── PostgresRepo.GetAccount (from)   10ms
│   ├── PostgresRepo.GetAccount (to)      9ms
│   ├── PostgresRepo.SaveTransfer       180ms  ← bottleneck!
│   └── EventBus.Publish                 6ms
└── Serialise response        3ms
```

A single `traceId` ties together all spans across services. Each span records: operation name, start time, duration, status, and attributes.

---

## OpenTelemetry Setup

OpenTelemetry is the vendor-neutral standard. You write OTel code once and choose your backend (Jaeger, Grafana Tempo, Datadog, AWS X-Ray, Azure Application Insights).

```xml
<!-- Bank.Api.csproj -->
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.*" />
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.*" />
<PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.*" />
<PackageReference Include="OpenTelemetry.Instrumentation.EntityFrameworkCore" Version="1.*" />
<PackageReference Include="OpenTelemetry.Exporter.Otlp" Version="1.*" />
```

```csharp
// Program.cs
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r
        .AddService("bank-api", serviceVersion: "1.4.2")
        .AddAttributes([("environment", builder.Environment.EnvironmentName)]))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()        // auto-instruments HTTP requests
        .AddHttpClientInstrumentation()         // auto-instruments outbound HTTP
        .AddEntityFrameworkCoreInstrumentation() // auto-instruments EF Core queries
        .AddOtlpExporter(o => o.Endpoint = new Uri("http://localhost:4317")))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddPrometheusExporter());             // exposes /metrics endpoint

app.MapPrometheusScrapingEndpoint(); // GET /metrics
```

---

## Trace Propagation

The `traceparent` header carries the trace context between services:

```
Client → Bank.Api: POST /v1/transfers
  traceparent: 00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01

Bank.Api → Bank.Repository (EF Core): handled automatically by OTel instrumentation
  traceparent: 00-4bf92f3577b34da6a3ce929d0e0e4736-b9c7c989f97918e1-01

Bank.Api → Temporal (StartWorkflow): propagated via OTel activity
  traceparent: 00-4bf92f3577b34da6a3ce929d0e0e4736-0f9c28b1c0d8a123-01
```

All spans share `traceId = 4bf92f3577b34da6a3ce929d0e0e4736`.

---

## Custom Spans for Business Operations

```csharp
public class BankService(IBankRepository repo, ILogger<BankService> logger)
{
    private static readonly ActivitySource _activitySource = new("BankService");

    public async Task<Transfer> CreateTransferAsync(
        Guid fromId, Guid toId, decimal amount, CancellationToken ct)
    {
        using var activity = _activitySource.StartActivity("CreateTransfer");
        activity?.SetTag("from.account_id", fromId.ToString());
        activity?.SetTag("to.account_id", toId.ToString());
        activity?.SetTag("transfer.amount", amount.ToString());

        try
        {
            var transfer = await repo.CreateTransferAsync(fromId, toId, amount, ct);
            activity?.SetTag("transfer.id", transfer.Id.ToString());
            activity?.SetStatus(ActivityStatusCode.Ok);
            return transfer;
        }
        catch (Exception ex)
        {
            activity?.RecordException(ex);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}

// Register the ActivitySource
builder.Services.AddOpenTelemetry().WithTracing(t =>
    t.AddSource("BankService"));
```

---

## Prometheus Metrics: The Four Golden Signals

```csharp
// Custom metrics (prometheus-net or OTel Metrics API)
public static class ApiMetrics
{
    private static readonly Histogram<double> _requestDuration =
        new Meter("BankApi").CreateHistogram<double>(
            "http_request_duration_seconds",
            "seconds",
            "Duration of HTTP requests");

    private static readonly Counter<long> _requestCount =
        new Meter("BankApi").CreateCounter<long>(
            "http_requests_total",
            "requests",
            "Total number of HTTP requests");

    private static readonly Counter<long> _errorCount =
        new Meter("BankApi").CreateCounter<long>(
            "http_request_errors_total",
            "errors",
            "Total number of failed HTTP requests");
}
```

| Signal | Metric Type | Example Query |
| :--- | :--- | :--- |
| **Latency** | Histogram | `histogram_quantile(0.99, rate(http_request_duration_seconds_bucket[5m]))` |
| **Traffic** | Counter | `rate(http_requests_total[5m])` |
| **Errors** | Counter | `rate(http_request_errors_total[5m]) / rate(http_requests_total[5m])` |
| **Saturation** | Gauge | `dotnet_threadpool_queue_length` (from OTel runtime instrumentation) |

---

## Correlating Logs and Traces

Inject `traceId` into every log line so you can pivot from a Jaeger trace to matching log entries in Grafana.

```csharp
// Middleware — inject trace context into Serilog's LogContext
public class TracingEnricherMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = Activity.Current?.TraceId.ToString();
        var spanId = Activity.Current?.SpanId.ToString();

        using (LogContext.PushProperty("TraceId", traceId))
        using (LogContext.PushProperty("SpanId", spanId))
        {
            await next(context);
        }
    }
}
```

Now every Serilog log line automatically includes `TraceId` and `SpanId`. In Grafana:
1. Click a slow trace in Jaeger → copy `traceId`
2. Query logs: `{service="bank-api"} | json | traceId = "4bf92f3577b34da6a3ce929d0e0e4736"`

---

## Full Observability Stack

```
Bank.Api
├── Serilog          → stdout JSON → Fluent Bit → Elasticsearch / Loki
├── /metrics         → Prometheus scrapes every 15s → Grafana dashboards
└── OTLP exporter    → Jaeger / Grafana Tempo → distributed trace UI
```

All three signals are correlated by `traceId`.

---

## Further Reading

- [OpenTelemetry .NET](https://opentelemetry.io/docs/languages/dotnet/)
- [Serilog + OTel](https://github.com/serilog-contrib/serilog-sinks-opentelemetry)
- [prometheus-net](https://github.com/prometheus-net/prometheus-net)
- [Jaeger for local dev](https://www.jaegertracing.io/docs/getting-started/)
