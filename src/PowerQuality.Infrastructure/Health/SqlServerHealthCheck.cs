using Microsoft.Extensions.Diagnostics.HealthChecks;
using PowerQuality.Infrastructure.Persistence;
namespace PowerQuality.Infrastructure.Health;
public sealed class SqlServerHealthCheck(IServiceScopeFactory scopeFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<PowerQualityDbContext>();
            return await db.Database.CanConnectAsync(ct) ? HealthCheckResult.Healthy("SQL Server reachable") : HealthCheckResult.Unhealthy("SQL Server unreachable");
        }
        catch (Exception ex) { return HealthCheckResult.Unhealthy("SQL Server health check failed", ex); }
    }
}
