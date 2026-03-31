using Bank.Repository;
using Microsoft.EntityFrameworkCore;
using Temporal.Worker;

var builder = Host.CreateApplicationBuilder(args);

// ── Database (EF Core + Postgres) ─────────────────────────────────────────────
builder.Services.AddDbContext<BankDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Default")
        ?? "Host=localhost;Port=5432;Database=dotnetbank;Username=dotnettrainer;Password=verysecret"));

builder.Services.AddScoped<IBankRepository, PostgresBankRepository>();

// ── Workers ────────────────────────────────────────────────────────────────────
builder.Services.AddHostedService<TemporalWorkerService>();
builder.Services.AddHostedService<DurableTransferWorkerService>();

var host = builder.Build();
host.Run();

