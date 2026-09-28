using PowerQuality.Application.DTOs;
using PowerQuality.Application.Exceptions;
namespace PowerQuality.Application.Validation;
public static class TelemetryValidator
{
    public static void Validate(TelemetryIngestRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.EventId)) throw new ValidationException("EVENT_ID_REQUIRED", "EventId is required.");
        if (r.DeviceId == Guid.Empty) throw new ValidationException("DEVICE_REQUIRED", "DeviceId is required.");
        if (r.Voltage is < 0 or > 1000) throw new ValidationException("INVALID_VOLTAGE", "Voltage must be between 0 and 1000 V.");
        if (r.Current is < 0 or > 10000) throw new ValidationException("INVALID_CURRENT", "Current must be between 0 and 10000 A.");
        if (r.Frequency is < 40 or > 70) throw new ValidationException("INVALID_FREQUENCY", "Frequency must be between 40 and 70 Hz.");
        if (r.PowerFactor is < 0 or > 1) throw new ValidationException("INVALID_POWER_FACTOR", "Power factor must be between 0 and 1.");
        if (r.TimestampUtc == default) throw new ValidationException("TIMESTAMP_REQUIRED", "TimestampUtc is required.");
        if (r.TimestampUtc > DateTime.UtcNow.AddMinutes(5)) throw new ValidationException("TIMESTAMP_IN_FUTURE", "TimestampUtc cannot be more than five minutes in the future.");
    }
}
