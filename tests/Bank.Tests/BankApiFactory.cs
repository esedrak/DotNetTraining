using Bank.Repository;
using Bank.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Bank.Tests;

/// <summary>
/// Custom <see cref="WebApplicationFactory{TEntryPoint}"/> that:
/// <list type="bullet">
///   <item>Swaps out the real Postgres <see cref="BankDbContext"/> for an in-memory one.</item>
///   <item>Replaces the real <see cref="IBankService"/> with a <see cref="Mock{T}"/>
///         so tests can control what the service returns without touching a database.</item>
/// </list>
///
/// Usage: implement <see cref="IClassFixture{TFixture}"/> on your test class.
/// The factory is created once per test class; a fresh test class instance is created
/// for each test, so reset the mock at the top of each test body:
/// <code>factory.MockBankService.Reset();</code>
/// </summary>
public class BankApiFactory : WebApplicationFactory<Program>
{
    /// <summary>Shared mock — reset at the start of each test before arranging.</summary>
    public Mock<IBankService> MockBankService { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // ── Swap BankDbContext for an in-memory provider ───────────────────
            var dbDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<BankDbContext>));
            if (dbDescriptor is not null)
                services.Remove(dbDescriptor);

            // Also remove the DbContext registration itself
            var ctxDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(BankDbContext));
            if (ctxDescriptor is not null)
                services.Remove(ctxDescriptor);

            services.AddDbContext<BankDbContext>(options =>
                options.UseInMemoryDatabase("BankTests"));

            // ── Swap IBankService for a Moq mock ──────────────────────────────
            var svcDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IBankService));
            if (svcDescriptor is not null)
                services.Remove(svcDescriptor);

            services.AddScoped<IBankService>(_ => MockBankService.Object);
        });
    }
}
