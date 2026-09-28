using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PowerQuality.Application.DTOs;
using PowerQuality.Application.Interfaces;
using PowerQuality.Application.Rules;
using PowerQuality.Domain.Entities;
using PowerQuality.Domain.Enums;
using PowerQuality.Infrastructure.Persistence;
namespace PowerQuality.Infrastructure.Services;
public sealed class PowerEventProcessor(PowerQualityDbContext db, IEnumerable<IPowerQualityRule> rules, IRealtimeNotifier realtime, INotificationClient notifications, IDeviceStateCache cache, IPlatformMetrics metrics, ILogger<PowerEventProcessor> logger) : IPowerEventProcessor
{
    public async Task<IReadOnlyList<long>> GetPendingMeasurementIdsAsync(int take, CancellationToken ct) =>
        await db.Measurements.AsNoTracking().Where(x => x.ProcessedAtUtc == null).OrderBy(x => x.Id).Take(Math.Clamp(take, 1, 5000)).Select(x => x.Id).ToListAsync(ct);

    public async Task ProcessAsync(long measurementId, CancellationToken ct)
    {
        var measurement = await db.Measurements.Include(x => x.Device).SingleOrDefaultAsync(x => x.Id == measurementId, ct);
        if (measurement is null || measurement.ProcessedAtUtc.HasValue) return;
        var device = measurement.Device!;
        var createdAlerts = new List<Alert>();
        await using var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        foreach (var rule in rules)
        {
            var result = rule.Evaluate(measurement);
            if (result is null) continue;
            var powerEvent = new PowerEvent
            {
                DeviceId = device.Id, MeasurementId = measurement.Id, Type = result.Type, Severity = result.Severity,
                Value = result.Value, Message = result.Message
            };
            db.PowerEvents.Add(powerEvent);
            var alert = new Alert
            {
                DeviceId = device.Id, PowerEvent = powerEvent, Type = result.Type, Severity = result.Severity, Message = result.Message
            };
            db.Alerts.Add(alert); createdAlerts.Add(alert);
            if (result.Severity == Severity.Critical) device.Status = DeviceStatus.Fault;
            else if (device.Status != DeviceStatus.Fault) device.Status = DeviceStatus.Warning;
            db.AuditLogs.Add(new AuditLog { Action = "PowerEventDetected", EntityType = "Device", EntityId = device.Id.ToString(), Details = $"{result.Type}: {result.Message}" });
        }
        measurement.ProcessedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        if (tx is not null) await tx.CommitAsync(ct);

        if (createdAlerts.Count > 0)
        {
            await cache.SetAsync(new PowerQuality.Application.Models.DeviceLiveState(device.Id, measurement.Voltage, measurement.Current, measurement.Frequency, measurement.PowerFactor, measurement.ActivePower, measurement.TimestampUtc, device.Status), ct);
            await realtime.DeviceStatusChangedAsync(new DeviceDto(device.Id, device.SiteId, device.SerialNumber, device.Name, device.FirmwareVersion, device.Status, device.LastSeenAtUtc), ct);
        }

        foreach (var alert in createdAlerts)
        {
            await db.Entry(alert).Reference(x => x.Device).LoadAsync(ct);
            var dto = new AlertDto(alert.Id, alert.DeviceId, alert.Device?.Name ?? device.Name, alert.Type, alert.Severity, alert.Status, alert.Message, alert.CreatedAtUtc, alert.AcknowledgedAtUtc, alert.ResolvedAtUtc);
            await realtime.AlertCreatedAsync(dto, ct);
            await notifications.SendAlertAsync(dto, ct);
            metrics.AlertCreated();
            logger.LogWarning("Power-quality alert {Type} created for device {DeviceId}; value={Value}", alert.Type, device.Id, alert.PowerEvent?.Value);
        }
    }
}
