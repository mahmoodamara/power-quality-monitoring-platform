using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;
namespace PowerQuality.Infrastructure.Health;
public sealed class RedisHealthCheck(IConnectionMultiplexer redis) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        try { await redis.GetDatabase().PingAsync(); return HealthCheckResult.Healthy("Redis reachable"); }
        catch (Exception ex) { return HealthCheckResult.Unhealthy("Redis health check failed", ex); }
    }
}
