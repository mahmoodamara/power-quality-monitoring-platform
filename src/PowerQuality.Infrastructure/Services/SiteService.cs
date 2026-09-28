using Microsoft.EntityFrameworkCore;
using PowerQuality.Application.DTOs;
using PowerQuality.Application.Exceptions;
using PowerQuality.Application.Interfaces;
using PowerQuality.Domain.Entities;
using PowerQuality.Infrastructure.Persistence;
namespace PowerQuality.Infrastructure.Services;
public sealed class SiteService(PowerQualityDbContext db) : ISiteService
{
    public async Task<IReadOnlyList<SiteDto>> GetAllAsync(CancellationToken ct) => await db.Sites.AsNoTracking().OrderBy(x => x.Name).Select(x => new SiteDto(x.Id, x.Name, x.Location, x.CreatedAtUtc)).ToListAsync(ct);
    public async Task<SiteDto> CreateAsync(CreateSiteRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) throw new ValidationException("SITE_NAME_REQUIRED", "Site name is required.");
        var site = new Site { Name = request.Name.Trim(), Location = request.Location?.Trim() };
        db.Sites.Add(site); await db.SaveChangesAsync(ct);
        return new(site.Id, site.Name, site.Location, site.CreatedAtUtc);
    }
}
