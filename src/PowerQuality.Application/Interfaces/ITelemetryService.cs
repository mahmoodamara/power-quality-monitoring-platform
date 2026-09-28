using PowerQuality.Application.DTOs;
namespace PowerQuality.Application.Interfaces;
public interface ITelemetryService
{
    Task<TelemetryIngestResponse> IngestAsync(TelemetryIngestRequest request, string? correlationId, CancellationToken cancellationToken);
    Task<IReadOnlyList<MeasurementDto>> GetMeasurementsAsync(Guid deviceId, DateTime? fromUtc, int limit, CancellationToken cancellationToken);
}
