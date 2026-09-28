using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PowerQuality.Application.Interfaces;
namespace PowerQuality.Worker;
public sealed class OfflineDetectionWorker(IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<OfflineDetectionWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var thresholdSeconds = configuration.GetValue("Monitoring:OfflineAfterSeconds", 60);
        var intervalSeconds = configuration.GetValue("Monitoring:OfflineCheckIntervalSeconds", 15);
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(intervalSeconds));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var service = scope.ServiceProvider.GetRequiredService<IOfflineDetectionService>();
                await service.DetectAsync(TimeSpan.FromSeconds(thresholdSeconds), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { logger.LogError(ex, "Offline detection cycle failed"); }
        }
    }
}
