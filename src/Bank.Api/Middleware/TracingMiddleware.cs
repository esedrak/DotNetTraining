namespace Bank.Api.Middleware;

/// <summary>
/// Propagates a correlation ID through every HTTP request and response.
/// If the caller sends <c>X-Correlation-Id</c> it is preserved; otherwise a
/// new <see cref="Guid"/> is generated. The ID is written back on the response
/// so clients can correlate their own logs.
///
/// Go equivalent: middleware that reads / writes <c>X-Correlation-Id</c> and
/// stores it in <c>context.Context</c>.
/// </summary>
public class TracingMiddleware(RequestDelegate next)
{
    private const string CorrelationHeader = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationHeader].FirstOrDefault()
                            ?? Guid.NewGuid().ToString("N");

        // Store on the request for downstream middleware / controllers
        context.Items[CorrelationHeader] = correlationId;

        // Echo back on the response so callers can correlate
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationHeader] = correlationId;
            return Task.CompletedTask;
        });

        await next(context);
    }
}
