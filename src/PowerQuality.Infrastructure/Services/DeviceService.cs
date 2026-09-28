using Microsoft.EntityFrameworkCore;
using PowerQuality.Application.DTOs;
using PowerQuality.Application.Exceptions;
using PowerQuality.Application.Interfaces;
using PowerQuality.Application.Models;
using PowerQuality.Domain.Entities;
using PowerQuality.Infrastructure.Persistence;
namespace PowerQuality.Infrastructure.Services;
public sealed class DeviceService(PowerQualityDbContext db, IDeviceStateCache cache) : IDeviceService
{
    public async Task<IReadOnlyList<DeviceDto>> GetAllAsync(CancellationToken ct) => await db.Devices.AsNoTracking().OrderBy(x => x.Name).Select(x => Map(x)).ToListAsync(ct);

    public async Task<DeviceDto> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var d = await db.Devices.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new NotFoundException("DEVICE_NOT_FOUND", $"Device {id} was not found.");
        return Map(d);
    }

    public async Task<DeviceDto> CreateAsync(CreateDeviceRequest request, CancellationToken ct)
    {
        if (request.SiteId == Guid.Empty) throw new ValidationException("SITE_REQUIRED", "SiteId is required.");
        if (string.IsNullOrWhiteSpace(request.SerialNumber) || string.IsNullOrWhiteSpace(request.Name)) throw new ValidationException("DEVICE_FIELDS_REQUIRED", "Serial number and name are required.");
        if (!await db.Sites.AnyAsync(x => x.Id == request.SiteId, ct)) throw new NotFoundException("SITE_NOT_FOUND", $"Site {request.SiteId} was not found.");
        if (await db.Devices.AnyAsync(x => x.SerialNumber == request.SerialNumber, ct)) throw new ConflictException("DUPLICATE_SERIAL", $"Serial number {request.SerialNumber} already exists.");
        var d = new Device { SiteId = request.SiteId, SerialNumber = request.SerialNumber.Trim(), Name = request.Name.Trim(), FirmwareVersion = string.IsNullOrWhiteSpace(request.FirmwareVersion) ? "1.0.0" : request.FirmwareVersion.Trim() };
        db.Devices.Add(d); await db.SaveChangesAsync(ct); return Map(d);
    }

    public async Task<DeviceLiveStateDto?> GetLiveStateAsync(Guid id, CancellationToken ct)
    {
        if (!await db.Devices.AnyAsync(x => x.Id == id, ct)) throw new NotFoundException("DEVICE_NOT_FOUND", $"Device {id} was not found.");
        var cached = await cache.GetAsync(id, ct);
        if (cached is not null) return new(cached.DeviceId, cached.Voltage, cached.Current, cached.Frequency, cached.PowerFactor, cached.ActivePower, cached.TimestampUtc, cached.Status);
        var latest = await db.Measurements.AsNoTracking().Where(x => x.DeviceId == id).OrderByDescending(x => x.TimestampUtc).FirstOrDefaultAsync(ct);
        if (latest is null) return null;
        var status = await db.Devices.AsNoTracking().Where(x => x.Id == id).Select(x => x.Status).SingleAsync(ct);
        var state = new DeviceLiveState(id, latest.Voltage, latest.Current, latest.Frequency, latest.PowerFactor, latest.ActivePower, latest.TimestampUtc, status);
        await cache.SetAsync(state, ct);
        return new(state.DeviceId, state.Voltage, state.Current, state.Frequency, state.PowerFactor, state.ActivePower, state.TimestampUtc, state.Status);
    }

    private static DeviceDto Map(Device d) => new(d.Id, d.SiteId, d.SerialNumber, d.Name, d.FirmwareVersion, d.Status, d.LastSeenAtUtc);
}
