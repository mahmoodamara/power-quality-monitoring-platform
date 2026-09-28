using PowerQuality.Domain.Enums;
namespace PowerQuality.Application.DTOs;
public sealed record PowerEventDto(long Id, Guid DeviceId, string DeviceName, PowerEventType Type, Severity Severity, decimal? Value, string Message, DateTime CreatedAtUtc);
