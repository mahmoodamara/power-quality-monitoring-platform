using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PowerQuality.Domain.Entities;
using PowerQuality.Domain.Enums;
namespace PowerQuality.Infrastructure.Persistence;
public static class DbInitializer
{
    public static async Task InitializeAsync(PowerQualityDbContext db, ILogger logger, CancellationToken cancellationToken = default)
    {
        Exception? last = null;
        for (var attempt = 1; attempt <= 12; attempt++)
        {
            try
            {
                await db.Database.EnsureCreatedAsync(cancellationToken);
                last = null;
                break;
            }
            catch (Exception ex)
            {
                last = ex;
                logger.LogWarning(ex, "Database not ready. Attempt {Attempt}/12", attempt);
                await Task.Delay(TimeSpan.FromSeconds(Math.Min(attempt * 2, 10)), cancellationToken);
            }
        }
        if (last is not null) throw last;

        if (await db.Sites.AnyAsync(cancellationToken)) return;

        var site = new Site { Name = "Caesarea Demo Plant", Location = "Caesarea, Israel" };
        var d1 = new Device { Site = site, SerialNumber = "MTR-1001", Name = "Main Incoming Meter", FirmwareVersion = "2.4.1", Status = DeviceStatus.Online, LastSeenAtUtc = DateTime.UtcNow };
        var d2 = new Device { Site = site, SerialNumber = "MTR-1002", Name = "Production Line Meter", FirmwareVersion = "2.4.1", Status = DeviceStatus.Online, LastSeenAtUtc = DateTime.UtcNow };
        db.AddRange(site, d1, d2);
        await db.SaveChangesAsync(cancellationToken);
    }
}
