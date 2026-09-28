using PowerQuality.Domain.Enums;
namespace PowerQuality.Application.DTOs;
public sealed record AlertDto(long Id, Guid DeviceId, string DeviceName, PowerEventType Type, Severity Severity, AlertStatus Status, string Message, DateTime CreatedAtUtc, DateTime? AcknowledgedAtUtc, DateTime? ResolvedAtUtc);
public sealed record AcknowledgeAlertRequest(string AcknowledgedBy, string? Note);
