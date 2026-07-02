using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AlpineGearHub.Chat.Infrastructure.Data;

public class DesignTimeChatDbContextFactory : IDesignTimeDbContextFactory<ChatDbContext>
{
    public ChatDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ChatDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=alpinegearhub;Username=alpinegearhub;Password=alpinegearhub")
            .Options;

        return new ChatDbContext(options);
    }
}
