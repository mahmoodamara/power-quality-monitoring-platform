using PowerQuality.Application.Models;
namespace PowerQuality.Application.Interfaces;
public interface IDeviceStateCache
{
    Task SetAsync(DeviceLiveState state, CancellationToken cancellationToken);
    Task<DeviceLiveState?> GetAsync(Guid deviceId, CancellationToken cancellationToken);
}
