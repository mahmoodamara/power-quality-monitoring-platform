using System.Threading.Channels;
using PowerQuality.Application.Interfaces;
using PowerQuality.Application.Models;
namespace PowerQuality.Worker;
public sealed class MeasurementQueue : IMeasurementQueue
{
    private readonly Channel<MeasurementEnvelope> _channel = Channel.CreateBounded<MeasurementEnvelope>(new BoundedChannelOptions(5000)
    {
        FullMode = BoundedChannelFullMode.Wait, SingleReader = true, SingleWriter = false, AllowSynchronousContinuations = false
    });
    public ValueTask EnqueueAsync(MeasurementEnvelope item, CancellationToken ct) => _channel.Writer.WriteAsync(item, ct);
    public IAsyncEnumerable<MeasurementEnvelope> ReadAllAsync(CancellationToken ct) => _channel.Reader.ReadAllAsync(ct);
}
