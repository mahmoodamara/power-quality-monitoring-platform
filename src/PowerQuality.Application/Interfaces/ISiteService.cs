using PowerQuality.Application.DTOs;
namespace PowerQuality.Application.Interfaces;
public interface ISiteService
{
    Task<IReadOnlyList<SiteDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<SiteDto> CreateAsync(CreateSiteRequest request, CancellationToken cancellationToken);
}
