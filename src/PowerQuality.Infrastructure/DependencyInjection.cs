using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using PowerQuality.Application.Interfaces;
using PowerQuality.Application.Rules;
using PowerQuality.Infrastructure.Health;
using PowerQuality.Infrastructure.Persistence;
using PowerQuality.Infrastructure.Services;
using StackExchange.Redis;
namespace PowerQuality.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var sql = configuration.GetConnectionString("SqlServer") ?? "Server=localhost,1433;Database=PowerQualityDb;User Id=sa;Password=PowerQuality123!;TrustServerCertificate=True;Encrypt=False;";
        var redis = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddDbContext<PowerQualityDbContext>(o => o.UseSqlServer(sql));
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(new ConfigurationOptions { EndPoints = { redis }, AbortOnConnectFail = false, ConnectRetry = 3 }));
        services.AddSingleton<IDeviceStateCache, RedisDeviceStateCache>();
        services.AddSingleton<IPlatformMetrics, PlatformMetrics>();
        services.AddScoped<ISiteService, SiteService>();
        services.AddScoped<IDeviceService, DeviceService>();
        services.AddScoped<ITelemetryService, TelemetryService>();
        services.AddScoped<IAlertService, AlertService>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IStatisticsService, StatisticsService>();
        services.AddScoped<IPowerEventProcessor, PowerEventProcessor>();
        services.AddScoped<IOfflineDetectionService, OfflineDetectionService>();
        services.AddSingleton<IPowerQualityRule, OverVoltageRule>();
        services.AddSingleton<IPowerQualityRule, UnderVoltageRule>();
        services.AddSingleton<IPowerQualityRule, FrequencyDeviationRule>();
        services.AddSingleton<IPowerQualityRule, LowPowerFactorRule>();
        services.AddHttpClient<INotificationClient, WebhookNotificationClient>()
            .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4) }))
            .AddTransientHttpErrorPolicy(p => p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));
        services.AddHealthChecks()
            .AddCheck<SqlServerHealthCheck>("sql-server", tags: new[] { "ready" })
            .AddCheck<RedisHealthCheck>("redis", tags: new[] { "ready" });
        return services;
    }
}
