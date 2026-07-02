using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AlpineGearHub.Moderation.Infrastructure.Data;

public class DesignTimeModerationDbContextFactory : IDesignTimeDbContextFactory<ModerationDbContext>
{
    public ModerationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ModerationDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=alpinegearhub;Username=alpinegearhub;Password=alpinegearhub")
            .Options;

        return new ModerationDbContext(options);
    }
}
