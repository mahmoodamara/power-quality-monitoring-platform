using PowerQuality.Application.DTOs;
namespace PowerQuality.Application.Interfaces;
public interface IDeviceService
{
    Task<IReadOnlyList<DeviceDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<DeviceDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<DeviceDto> CreateAsync(CreateDeviceRequest request, CancellationToken cancellationToken);
    Task<DeviceLiveStateDto?> GetLiveStateAsync(Guid id, CancellationToken cancellationToken);
}
