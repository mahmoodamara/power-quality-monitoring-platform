using PowerQuality.Application.DTOs;
namespace PowerQuality.Application.Interfaces;
public interface IEventService
{
    Task<IReadOnlyList<PowerEventDto>> GetAsync(Guid? deviceId, int limit, CancellationToken cancellationToken);
}
