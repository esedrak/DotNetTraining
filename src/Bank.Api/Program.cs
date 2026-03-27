using Bank.Api.Middleware;
using Bank.Repository;
using Bank.Service;
using Microsoft.EntityFrameworkCore;
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

// ── ASP.NET Core ─────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Correlation ID propagation — must be first so all subsequent middleware can use it
app.UseMiddleware<TracingMiddleware>();

// Structured request/response logging
app.UseMiddleware<LoggingMiddleware>();

// JWT auth — uncomment to enable (requires Jwt:Secret in appsettings.json)
// app.UseMiddleware<AuthMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Required for WebApplicationFactory in integration tests
public partial class Program { }
