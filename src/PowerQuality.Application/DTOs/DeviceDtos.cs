using PowerQuality.Domain.Enums;
namespace PowerQuality.Application.DTOs;
public sealed record DeviceDto(Guid Id, Guid SiteId, string SerialNumber, string Name, string FirmwareVersion, DeviceStatus Status, DateTime? LastSeenAtUtc);
public sealed record CreateDeviceRequest(Guid SiteId, string SerialNumber, string Name, string FirmwareVersion);
public sealed record DeviceLiveStateDto(Guid DeviceId, decimal Voltage, decimal Current, decimal Frequency, decimal PowerFactor, decimal ActivePower, DateTime TimestampUtc, DeviceStatus Status);
