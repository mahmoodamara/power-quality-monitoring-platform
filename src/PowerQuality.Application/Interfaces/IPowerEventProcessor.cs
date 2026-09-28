namespace PowerQuality.Application.Interfaces;
public interface IPowerEventProcessor
{
    Task ProcessAsync(long measurementId, CancellationToken cancellationToken);
    Task<IReadOnlyList<long>> GetPendingMeasurementIdsAsync(int take, CancellationToken cancellationToken);
}
