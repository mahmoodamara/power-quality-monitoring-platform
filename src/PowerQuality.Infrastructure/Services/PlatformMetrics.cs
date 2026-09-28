using System.Diagnostics.Metrics;
using PowerQuality.Application.Interfaces;
namespace PowerQuality.Infrastructure.Services;
public sealed class PlatformMetrics : IPlatformMetrics
{
    public const string MeterName = "PowerQuality.Metrics";
    private static readonly Meter Meter = new(MeterName, "1.0.0");
    private static readonly Counter<long> TelemetryReceivedCounter = Meter.CreateCounter<long>("telemetry_received_total");
    private static readonly Counter<long> TelemetryRejectedCounter = Meter.CreateCounter<long>("telemetry_rejected_total");
    private static readonly Counter<long> AlertsCreatedCounter = Meter.CreateCounter<long>("alerts_created_total");
    public void TelemetryReceived() => TelemetryReceivedCounter.Add(1);
    public void TelemetryRejected() => TelemetryRejectedCounter.Add(1);
    public void AlertCreated() => AlertsCreatedCounter.Add(1);
}
