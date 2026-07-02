using AlpineGearHub.Moderation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlpineGearHub.Moderation.Infrastructure.Data;

public class ModerationDbContext(DbContextOptions<ModerationDbContext> options) : DbContext(options)
{
    public DbSet<Report> Reports => Set<Report>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("moderation");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ModerationDbContext).Assembly);
    }
}
