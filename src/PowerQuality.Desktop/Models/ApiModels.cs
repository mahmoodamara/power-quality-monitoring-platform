namespace PowerQuality.Desktop.Models;
public sealed record DeviceDto(Guid Id, Guid SiteId, string SerialNumber, string Name, string FirmwareVersion, string Status, DateTime? LastSeenAtUtc);
public sealed record DeviceLiveStateDto(Guid DeviceId, decimal Voltage, decimal Current, decimal Frequency, decimal PowerFactor, decimal ActivePower, DateTime TimestampUtc, string Status);
public sealed record AlertDto(long Id, Guid DeviceId, string DeviceName, string Type, string Severity, string Status, string Message, DateTime CreatedAtUtc, DateTime? AcknowledgedAtUtc, DateTime? ResolvedAtUtc);
