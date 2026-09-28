using PowerQuality.Application.DTOs;
namespace PowerQuality.Application.Interfaces;
public interface IStatisticsService
{
    Task<DeviceStatisticsDto> GetAsync(Guid deviceId, int hours, CancellationToken cancellationToken);
}
