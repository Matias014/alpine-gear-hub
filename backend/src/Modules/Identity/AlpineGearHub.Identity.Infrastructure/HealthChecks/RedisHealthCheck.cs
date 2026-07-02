using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace AlpineGearHub.Identity.Infrastructure.HealthChecks;

internal sealed class RedisHealthCheck(IConnectionMultiplexer redis) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var latency = await redis.GetDatabase().PingAsync();
            return HealthCheckResult.Healthy($"Redis responded in {latency.TotalMilliseconds}ms");
        }
        catch (Exception ex)
        {
            // Didn't bother with a custom timeout here - StackExchange.Redis already has its own
            // connect/sync timeouts configured, so a hung Redis just surfaces as this exception.
            return HealthCheckResult.Unhealthy("Redis ping failed", ex);
        }
    }
}
