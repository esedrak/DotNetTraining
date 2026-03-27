using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Bank.Repository;

public class BankDbContextFactory : IDesignTimeDbContextFactory<BankDbContext>
{
    public BankDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<BankDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=dotnetbank;Username=dotnettrainer;Password=verysecret")
            .Options;

        return new BankDbContext(options);
    }
}
