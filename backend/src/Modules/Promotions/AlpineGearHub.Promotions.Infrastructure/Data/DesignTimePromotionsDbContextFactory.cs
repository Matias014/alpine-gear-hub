using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AlpineGearHub.Promotions.Infrastructure.Data;

public class DesignTimePromotionsDbContextFactory : IDesignTimeDbContextFactory<PromotionsDbContext>
{
    public PromotionsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<PromotionsDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=alpinegearhub;Username=alpinegearhub;Password=alpinegearhub")
            .Options;

        return new PromotionsDbContext(options);
    }
}
