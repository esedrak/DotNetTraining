using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Bank.Api.Middleware;

/// <summary>
/// Validates a Bearer JWT on protected routes.
/// Skips health and OpenAPI endpoints so they remain public.
///
/// NOTE: In production prefer <c>builder.Services.AddAuthentication().AddJwtBearer()</c>
/// — this class is a teaching example showing what the framework does internally.
///
/// Go equivalent: JWT validation middleware for Gin or <c>net/http</c>.
/// </summary>
public class AuthMiddleware(RequestDelegate next, IConfiguration config, ILogger<AuthMiddleware> logger)
{
    private static readonly string[] PublicPaths = ["/health", "/openapi", "/scalar"];

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";

        // Skip auth for public endpoints
        if (PublicPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await next(context);
            return;
        }

        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (authHeader is null || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = "Missing or invalid Authorization header." });
            return;
        }

        var token = authHeader["Bearer ".Length..].Trim();
        var principal = ValidateToken(token, config);

        if (principal is null)
        {
            logger.LogWarning("JWT validation failed for path {Path}", path);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = "Invalid or expired token." });
            return;
        }

        context.User = principal;
        await next(context);
    }

    private static ClaimsPrincipal? ValidateToken(string token, IConfiguration config)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var secret = config["Jwt:Secret"] ?? "change-me-in-production-must-be-32-chars!!";
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secret));

            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = false,   // set ValidIssuer in production
                ValidateAudience = false, // set ValidAudience in production
                ClockSkew = TimeSpan.FromSeconds(30)
            };

            return handler.ValidateToken(token, parameters, out _);
        }
        catch
        {
            return null;
        }
    }
}
