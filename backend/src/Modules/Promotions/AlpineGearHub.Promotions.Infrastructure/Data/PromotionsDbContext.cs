using AlpineGearHub.Promotions.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlpineGearHub.Promotions.Infrastructure.Data;

public class PromotionsDbContext(DbContextOptions<PromotionsDbContext> options) : DbContext(options)
{
    public DbSet<Promotion> Promotions => Set<Promotion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("promotions");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PromotionsDbContext).Assembly);
    }
}
