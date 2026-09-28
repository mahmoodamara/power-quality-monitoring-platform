using PowerQuality.Domain.Enums;
namespace PowerQuality.Application.Models;
public sealed record DeviceLiveState(Guid DeviceId, decimal Voltage, decimal Current, decimal Frequency, decimal PowerFactor, decimal ActivePower, DateTime TimestampUtc, DeviceStatus Status);
