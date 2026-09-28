using Microsoft.Extensions.DependencyInjection;
using PowerQuality.Application.Interfaces;
namespace PowerQuality.Worker;
public static class DependencyInjection
{
    public static IServiceCollection AddPowerQualityWorkers(this IServiceCollection services)
    {
        services.AddSingleton<IMeasurementQueue, MeasurementQueue>();
        services.AddHostedService<TelemetryProcessingWorker>();
        services.AddHostedService<OfflineDetectionWorker>();
        return services;
    }
}
