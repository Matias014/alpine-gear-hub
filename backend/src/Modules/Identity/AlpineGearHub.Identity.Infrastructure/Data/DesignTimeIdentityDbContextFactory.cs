using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AlpineGearHub.Identity.Infrastructure.Data;

public class DesignTimeIdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=alpinegearhub;Username=alpinegearhub;Password=alpinegearhub")
            .Options;
        return new IdentityDbContext(options);
    }
}
