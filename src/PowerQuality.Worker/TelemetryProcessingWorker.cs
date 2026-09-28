using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PowerQuality.Application.Interfaces;
using PowerQuality.Application.Models;
namespace PowerQuality.Worker;
public sealed class TelemetryProcessingWorker(IMeasurementQueue queue, IServiceScopeFactory scopeFactory, ILogger<TelemetryProcessingWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RecoverPendingAsync(stoppingToken);
        await foreach (var item in queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var processor = scope.ServiceProvider.GetRequiredService<IPowerEventProcessor>();
                await processor.ProcessAsync(item.MeasurementId, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex)
            {
                logger.LogError(ex, "Telemetry processing failed for measurement {MeasurementId}, attempt {Attempt}", item.MeasurementId, item.Attempt + 1);
                if (item.Attempt < 2)
                {
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, item.Attempt)), stoppingToken);
                    await queue.EnqueueAsync(item with { Attempt = item.Attempt + 1 }, stoppingToken);
                }
            }
        }
    }

    private async Task RecoverPendingAsync(CancellationToken ct)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var processor = scope.ServiceProvider.GetRequiredService<IPowerEventProcessor>();
            var pending = await processor.GetPendingMeasurementIdsAsync(5000, ct);
            foreach (var id in pending) await queue.EnqueueAsync(new MeasurementEnvelope(id), ct);
            if (pending.Count > 0) logger.LogInformation("Recovered {Count} unprocessed measurements", pending.Count);
        }
        catch (Exception ex) { logger.LogError(ex, "Failed to recover pending measurements. New telemetry will still be processed."); }
    }
}
