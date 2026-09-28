using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PowerQuality.Application.DTOs;
using PowerQuality.Application.Interfaces;
using PowerQuality.Domain.Entities;
using PowerQuality.Domain.Enums;
using PowerQuality.Infrastructure.Persistence;
namespace PowerQuality.Infrastructure.Services;
public sealed class OfflineDetectionService(PowerQualityDbContext db, IRealtimeNotifier realtime, INotificationClient notifications, IDeviceStateCache cache, IPlatformMetrics metrics, ILogger<OfflineDetectionService> logger) : IOfflineDetectionService
{
    public async Task<int> DetectAsync(TimeSpan offlineAfter, CancellationToken ct)
    {
        var cutoff = DateTime.UtcNow.Subtract(offlineAfter);
        var devices = await db.Devices.Where(x => x.Status != DeviceStatus.Offline && (!x.LastSeenAtUtc.HasValue || x.LastSeenAtUtc < cutoff)).ToListAsync(ct);
        var count = 0;
        foreach (var device in devices)
        {
            var existing = await db.Alerts.AnyAsync(x => x.DeviceId == device.Id && x.Type == PowerEventType.DeviceOffline && x.Status != AlertStatus.Resolved, ct);
            device.Status = DeviceStatus.Offline;
            if (!existing)
            {
                var powerEvent = new PowerEvent { DeviceId = device.Id, Type = PowerEventType.DeviceOffline, Severity = Severity.Critical, Message = $"Device has not reported telemetry for more than {offlineAfter.TotalSeconds:F0} seconds." };
                var alert = new Alert { DeviceId = device.Id, PowerEvent = powerEvent, Type = PowerEventType.DeviceOffline, Severity = Severity.Critical, Message = powerEvent.Message };
                db.AddRange(powerEvent, alert);
                await db.SaveChangesAsync(ct);
                var dto = new AlertDto(alert.Id, device.Id, device.Name, alert.Type, alert.Severity, alert.Status, alert.Message, alert.CreatedAtUtc, null, null);
                await realtime.AlertCreatedAsync(dto, ct); await notifications.SendAlertAsync(dto, ct); metrics.AlertCreated(); count++;
            }
            var live = await cache.GetAsync(device.Id, ct);
            if (live is not null) await cache.SetAsync(live with { Status = DeviceStatus.Offline }, ct);
            await realtime.DeviceStatusChangedAsync(new DeviceDto(device.Id, device.SiteId, device.SerialNumber, device.Name, device.FirmwareVersion, device.Status, device.LastSeenAtUtc), ct);
        }
        if (devices.Count > 0) { await db.SaveChangesAsync(ct); logger.LogWarning("Marked {Count} devices offline", devices.Count); }
        return count;
    }
}
