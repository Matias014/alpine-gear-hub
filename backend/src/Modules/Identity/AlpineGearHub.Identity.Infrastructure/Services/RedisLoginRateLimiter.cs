using AlpineGearHub.Identity.Application.Interfaces;
using StackExchange.Redis;

namespace AlpineGearHub.Identity.Infrastructure.Services;

internal sealed class RedisLoginRateLimiter(IConnectionMultiplexer redis) : ILoginRateLimiter
{
    // Went with 5 attempts / 15 min after testing this in earlier phases with no limit at all -
    // tight enough to stop brute-forcing, loose enough a fat-fingered password twice won't lock anyone out.
    private const int MaxAttempts = 5;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(15);

    public async Task<bool> IsBlockedAsync(string email, CancellationToken ct = default)
    {
        var count = await redis.GetDatabase().StringGetAsync(Key(email));
        return count.HasValue && (long)count >= MaxAttempts;
    }

    public async Task RegisterFailedAttemptAsync(string email, CancellationToken ct = default)
    {
        var db = redis.GetDatabase();
        var key = Key(email);
        var count = await db.StringIncrementAsync(key);
        if (count == 1)
            await db.KeyExpireAsync(key, Window);
    }

    public Task ResetAsync(string email, CancellationToken ct = default) =>
        redis.GetDatabase().KeyDeleteAsync(Key(email));

    private static string Key(string email) => $"login-attempts:{email}";
}
