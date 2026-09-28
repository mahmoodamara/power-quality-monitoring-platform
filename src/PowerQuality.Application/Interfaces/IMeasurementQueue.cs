using PowerQuality.Application.Models;
namespace PowerQuality.Application.Interfaces;
public interface IMeasurementQueue
{
    ValueTask EnqueueAsync(MeasurementEnvelope item, CancellationToken cancellationToken);
    IAsyncEnumerable<MeasurementEnvelope> ReadAllAsync(CancellationToken cancellationToken);
}
