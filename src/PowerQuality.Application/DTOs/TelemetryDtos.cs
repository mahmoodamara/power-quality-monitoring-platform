namespace PowerQuality.Application.DTOs;
public sealed record TelemetryIngestRequest(string EventId, Guid DeviceId, decimal Voltage, decimal Current, decimal Frequency, decimal PowerFactor, decimal ActivePower, DateTime TimestampUtc);
public sealed record TelemetryIngestResponse(long MeasurementId, bool Duplicate, string Status);
public sealed record MeasurementDto(long Id, string ExternalEventId, Guid DeviceId, decimal Voltage, decimal Current, decimal Frequency, decimal PowerFactor, decimal ActivePower, DateTime TimestampUtc, DateTime ReceivedAtUtc);
