using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PowerQuality.Application.DTOs;
using PowerQuality.Application.Exceptions;
using PowerQuality.Application.Interfaces;
using PowerQuality.Application.Models;
using PowerQuality.Application.Validation;
using PowerQuality.Domain.Entities;
using PowerQuality.Domain.Enums;
using PowerQuality.Infrastructure.Persistence;
namespace PowerQuality.Infrastructure.Services;
public sealed class TelemetryService(PowerQualityDbContext db, IMeasurementQueue queue, IDeviceStateCache cache, IRealtimeNotifier realtime, IPlatformMetrics metrics, ILogger<TelemetryService> logger) : ITelemetryService
{
    public async Task<TelemetryIngestResponse> IngestAsync(TelemetryIngestRequest request, string? correlationId, CancellationToken ct)
    {
        try { TelemetryValidator.Validate(request); }
        catch { metrics.TelemetryRejected(); throw; }

        var existing = await db.Measurements.AsNoTracking().SingleOrDefaultAsync(x => x.ExternalEventId == request.EventId, ct);
        if (existing is not null) return new(existing.Id, true, "duplicate");

        var device = await db.Devices.SingleOrDefaultAsync(x => x.Id == request.DeviceId, ct) ?? throw new NotFoundException("DEVICE_NOT_FOUND", $"Device {request.DeviceId} was not found.");
        await using var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        try
        {
            var measurement = new Measurement
            {
                ExternalEventId = request.EventId.Trim(), DeviceId = request.DeviceId, Voltage = request.Voltage, Current = request.Current,
                Frequency = request.Frequency, PowerFactor = request.PowerFactor, ActivePower = request.ActivePower,
                TimestampUtc = request.TimestampUtc.Kind == DateTimeKind.Utc ? request.TimestampUtc : request.TimestampUtc.ToUniversalTime()
            };
            db.Measurements.Add(measurement);
            device.LastSeenAtUtc = DateTime.UtcNow;
            device.Status = DeviceStatus.Online;

            var offlineAlerts = await db.Alerts.Where(x => x.DeviceId == device.Id && x.Type == PowerEventType.DeviceOffline && x.Status != AlertStatus.Resolved).ToListAsync(ct);
            foreach (var alert in offlineAlerts) { alert.Status = AlertStatus.Resolved; alert.ResolvedAtUtc = DateTime.UtcNow; }

            db.AuditLogs.Add(new AuditLog { Action = "TelemetryReceived", EntityType = "Device", EntityId = device.Id.ToString(), Details = $"EventId={request.EventId}", CorrelationId = correlationId });
            await db.SaveChangesAsync(ct);
            if (tx is not null) await tx.CommitAsync(ct);

            var state = new DeviceLiveState(device.Id, measurement.Voltage, measurement.Current, measurement.Frequency, measurement.PowerFactor, measurement.ActivePower, measurement.TimestampUtc, device.Status);
            await cache.SetAsync(state, ct);
            await queue.EnqueueAsync(new MeasurementEnvelope(measurement.Id), ct);
            await realtime.MeasurementReceivedAsync(state, ct);
            metrics.TelemetryReceived();
            logger.LogInformation("Telemetry {EventId} accepted for device {DeviceId}", request.EventId, request.DeviceId);
            return new(measurement.Id, false, "accepted");
        }
        catch (DbUpdateException)
        {
            if (tx is not null) await tx.RollbackAsync(ct);
            var duplicate = await db.Measurements.AsNoTracking().SingleOrDefaultAsync(x => x.ExternalEventId == request.EventId, ct);
            if (duplicate is not null) return new(duplicate.Id, true, "duplicate");
            metrics.TelemetryRejected(); throw;
        }
    }

    public async Task<IReadOnlyList<MeasurementDto>> GetMeasurementsAsync(Guid deviceId, DateTime? fromUtc, int limit, CancellationToken ct)
    {
        if (!await db.Devices.AnyAsync(x => x.Id == deviceId, ct)) throw new NotFoundException("DEVICE_NOT_FOUND", $"Device {deviceId} was not found.");
        limit = Math.Clamp(limit, 1, 1000);
        var query = db.Measurements.AsNoTracking().Where(x => x.DeviceId == deviceId);
        if (fromUtc.HasValue) query = query.Where(x => x.TimestampUtc >= fromUtc.Value);
        return await query.OrderByDescending(x => x.TimestampUtc).Take(limit).Select(x => new MeasurementDto(x.Id, x.ExternalEventId, x.DeviceId, x.Voltage, x.Current, x.Frequency, x.PowerFactor, x.ActivePower, x.TimestampUtc, x.ReceivedAtUtc)).ToListAsync(ct);
    }
}
