using System.Diagnostics;
using Serilog.Context;

namespace Bank.Api.Middleware;

/// <summary>
/// Logs every inbound HTTP request and its outcome.
/// Enriches the Serilog log context with request metadata so all log lines
/// emitted during the request carry the same correlation fields.
///
/// Go equivalent: custom <c>net/http</c> middleware logging via <c>slog</c>.
/// </summary>
public class LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                            ?? context.TraceIdentifier;

        // Push structured properties — all logs within this request carry these fields
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("RequestMethod", context.Request.Method))
        using (LogContext.PushProperty("RequestPath", context.Request.Path))
        {
            var sw = Stopwatch.StartNew();

            logger.LogInformation("HTTP {Method} {Path} started",
                context.Request.Method, context.Request.Path);

            try
            {
                await next(context);
            }
            finally
            {
                sw.Stop();
                logger.LogInformation(
                    "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    sw.ElapsedMilliseconds);
            }
        }
    }
}
