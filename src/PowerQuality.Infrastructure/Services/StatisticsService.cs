using Microsoft.EntityFrameworkCore;
using PowerQuality.Application.DTOs;
using PowerQuality.Application.Exceptions;
using PowerQuality.Application.Interfaces;
using PowerQuality.Infrastructure.Persistence;
namespace PowerQuality.Infrastructure.Services;
public sealed class StatisticsService(PowerQualityDbContext db) : IStatisticsService
{
    public async Task<DeviceStatisticsDto> GetAsync(Guid deviceId, int hours, CancellationToken ct)
    {
        if (!await db.Devices.AnyAsync(x => x.Id == deviceId, ct)) throw new NotFoundException("DEVICE_NOT_FOUND", $"Device {deviceId} was not found.");
        hours = Math.Clamp(hours, 1, 24 * 30);
        var from = DateTime.UtcNow.AddHours(-hours);
        var q = db.Measurements.AsNoTracking().Where(x => x.DeviceId == deviceId && x.TimestampUtc >= from);
        var sampleCount = await q.CountAsync(ct);
        decimal? avgV = null, minV = null, maxV = null, avgF = null, avgPf = null;
        if (sampleCount > 0)
        {
            avgV = await q.AverageAsync(x => x.Voltage, ct); minV = await q.MinAsync(x => x.Voltage, ct); maxV = await q.MaxAsync(x => x.Voltage, ct);
            avgF = await q.AverageAsync(x => x.Frequency, ct); avgPf = await q.AverageAsync(x => x.PowerFactor, ct);
        }
        var eventCount = await db.PowerEvents.AsNoTracking().CountAsync(x => x.DeviceId == deviceId && x.CreatedAtUtc >= from, ct);
        var device = await db.Devices.AsNoTracking().SingleAsync(x => x.Id == deviceId, ct);
        var uptime = device.LastSeenAtUtc.HasValue && device.LastSeenAtUtc.Value >= DateTime.UtcNow.AddMinutes(-2) ? 100d : sampleCount > 0 ? 95d : 0d;
        return new(deviceId, hours, sampleCount, avgV, minV, maxV, avgF, avgPf, eventCount, uptime);
    }
}
