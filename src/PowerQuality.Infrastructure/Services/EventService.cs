using Microsoft.EntityFrameworkCore;
using PowerQuality.Application.DTOs;
using PowerQuality.Application.Interfaces;
using PowerQuality.Infrastructure.Persistence;
namespace PowerQuality.Infrastructure.Services;
public sealed class EventService(PowerQualityDbContext db) : IEventService
{
    public async Task<IReadOnlyList<PowerEventDto>> GetAsync(Guid? deviceId, int limit, CancellationToken ct)
    {
        limit = Math.Clamp(limit, 1, 500);
        var q = db.PowerEvents.AsNoTracking().Include(x => x.Device).AsQueryable();
        if (deviceId.HasValue) q = q.Where(x => x.DeviceId == deviceId.Value);
        return await q.OrderByDescending(x => x.CreatedAtUtc).Take(limit).Select(x => new PowerEventDto(x.Id, x.DeviceId, x.Device!.Name, x.Type, x.Severity, x.Value, x.Message, x.CreatedAtUtc)).ToListAsync(ct);
    }
}
