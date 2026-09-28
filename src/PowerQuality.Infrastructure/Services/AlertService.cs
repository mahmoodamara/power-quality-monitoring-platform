using Microsoft.EntityFrameworkCore;
using PowerQuality.Application.DTOs;
using PowerQuality.Application.Exceptions;
using PowerQuality.Application.Interfaces;
using PowerQuality.Domain.Entities;
using PowerQuality.Domain.Enums;
using PowerQuality.Infrastructure.Persistence;
namespace PowerQuality.Infrastructure.Services;
public sealed class AlertService(PowerQualityDbContext db) : IAlertService
{
    public async Task<IReadOnlyList<AlertDto>> GetAsync(AlertStatus? status, int limit, CancellationToken ct)
    {
        limit = Math.Clamp(limit, 1, 500);
        var q = db.Alerts.AsNoTracking().Include(x => x.Device).AsQueryable();
        if (status.HasValue) q = q.Where(x => x.Status == status.Value);
        return await q.OrderByDescending(x => x.CreatedAtUtc).Take(limit).Select(x => new AlertDto(x.Id, x.DeviceId, x.Device!.Name, x.Type, x.Severity, x.Status, x.Message, x.CreatedAtUtc, x.AcknowledgedAtUtc, x.ResolvedAtUtc)).ToListAsync(ct);
    }

    public async Task<AlertDto> AcknowledgeAsync(long id, AcknowledgeAlertRequest request, string? correlationId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.AcknowledgedBy)) throw new ValidationException("ACK_USER_REQUIRED", "AcknowledgedBy is required.");
        var alert = await db.Alerts.Include(x => x.Device).SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new NotFoundException("ALERT_NOT_FOUND", $"Alert {id} was not found.");
        if (alert.Status == AlertStatus.Resolved) throw new ConflictException("ALERT_RESOLVED", "Resolved alerts cannot be acknowledged.");
        alert.Status = AlertStatus.Acknowledged; alert.AcknowledgedAtUtc = DateTime.UtcNow;
        db.AlertAcknowledgements.Add(new AlertAcknowledgement { AlertId = alert.Id, AcknowledgedBy = request.AcknowledgedBy.Trim(), Note = request.Note?.Trim() });
        db.AuditLogs.Add(new AuditLog { Action = "AlertAcknowledged", EntityType = "Alert", EntityId = alert.Id.ToString(), Details = request.Note, CorrelationId = correlationId });
        await db.SaveChangesAsync(ct);
        return new(alert.Id, alert.DeviceId, alert.Device!.Name, alert.Type, alert.Severity, alert.Status, alert.Message, alert.CreatedAtUtc, alert.AcknowledgedAtUtc, alert.ResolvedAtUtc);
    }
}
