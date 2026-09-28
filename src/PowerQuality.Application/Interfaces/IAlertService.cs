using PowerQuality.Application.DTOs;
using PowerQuality.Domain.Enums;
namespace PowerQuality.Application.Interfaces;
public interface IAlertService
{
    Task<IReadOnlyList<AlertDto>> GetAsync(AlertStatus? status, int limit, CancellationToken cancellationToken);
    Task<AlertDto> AcknowledgeAsync(long id, AcknowledgeAlertRequest request, string? correlationId, CancellationToken cancellationToken);
}
