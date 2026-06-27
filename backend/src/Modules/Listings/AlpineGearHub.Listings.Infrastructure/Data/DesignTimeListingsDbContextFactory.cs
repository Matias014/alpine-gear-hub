using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AlpineGearHub.Listings.Infrastructure.Data;

public class DesignTimeListingsDbContextFactory : IDesignTimeDbContextFactory<ListingsDbContext>
{
    public ListingsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ListingsDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=alpinegearhub;Username=alpinegearhub;Password=alpinegearhub")
            .Options;

        return new ListingsDbContext(options);
    }
}
