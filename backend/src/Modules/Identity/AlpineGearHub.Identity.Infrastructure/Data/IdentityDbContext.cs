using AlpineGearHub.Identity.Domain.Entities;
using AlpineGearHub.Identity.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace AlpineGearHub.Identity.Infrastructure.Data;

public class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("identity");
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
    }
}
