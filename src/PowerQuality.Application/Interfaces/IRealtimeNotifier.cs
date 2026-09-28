using PowerQuality.Application.DTOs;
using PowerQuality.Application.Models;
namespace PowerQuality.Application.Interfaces;
public interface IRealtimeNotifier
{
    Task MeasurementReceivedAsync(DeviceLiveState state, CancellationToken cancellationToken);
    Task AlertCreatedAsync(AlertDto alert, CancellationToken cancellationToken);
    Task DeviceStatusChangedAsync(DeviceDto device, CancellationToken cancellationToken);
}
