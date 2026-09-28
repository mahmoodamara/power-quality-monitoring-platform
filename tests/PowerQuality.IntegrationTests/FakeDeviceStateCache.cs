using PowerQuality.Application.Interfaces;
using PowerQuality.Application.Models;
namespace PowerQuality.IntegrationTests;
public sealed class FakeDeviceStateCache : IDeviceStateCache
{
    private readonly Dictionary<Guid, DeviceLiveState> _values = new();
    public Task SetAsync(DeviceLiveState state, CancellationToken ct) { _values[state.DeviceId] = state; return Task.CompletedTask; }
    public Task<DeviceLiveState?> GetAsync(Guid deviceId, CancellationToken ct) => Task.FromResult(_values.TryGetValue(deviceId, out var state) ? state : null);
}
