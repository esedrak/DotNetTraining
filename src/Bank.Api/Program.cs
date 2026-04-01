using System.Diagnostics;
using Bank.Api.Middleware;
using Scalar.AspNetCore;
using Bank.Repository;
using Bank.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ── Logging (Serilog) ────────────────────────────────────────────────────────
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"));

// ── Database (EF Core + Postgres) ────────────────────────────────────────────
builder.Services.AddDbContext<BankDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Default")
        ?? "Host=localhost;Port=5432;Database=dotnetbank;Username=dotnettrainer;Password=verysecret",
        npgsql => npgsql.MigrationsAssembly(typeof(BankDbContext).Assembly.FullName)));

// ── Dependency Injection ─────────────────────────────────────────────────────
builder.Services.AddScoped<IBankRepository, PostgresBankRepository>();
builder.Services.AddScoped<IBankService, BankService>();
builder.Services.AddSingleton(new ActivitySource("Bank.Api"));

// ── Authentication & Authorisation ───────────────────────────────────────────
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "change-me-in-production-must-be-32-chars!!";
var signingKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSecret));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateIssuer = false,   // set ValidIssuer in production
            ValidateAudience = false, // set ValidAudience in production
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

// ── ASP.NET Core ─────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();               // → GET /openapi/v1.json
    app.MapScalarApiReference();    // → GET /scalar/v1
}

// Correlation ID propagation — must be first so all subsequent middleware can use it
app.UseMiddleware<TracingMiddleware>();

// Structured request/response logging
app.UseMiddleware<LoggingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Required for WebApplicationFactory in integration tests
public partial class Program { }
