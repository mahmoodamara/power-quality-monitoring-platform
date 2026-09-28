using Microsoft.AspNetCore.Mvc;
using PowerQuality.Application.DTOs;
using PowerQuality.Application.Interfaces;
namespace PowerQuality.Api.Controllers;
[ApiController, Route("api/sites")]
public sealed class SitesController(ISiteService service) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<IReadOnlyList<SiteDto>>> Get(CancellationToken ct) => Ok(await service.GetAllAsync(ct));
    [HttpPost] public async Task<ActionResult<SiteDto>> Create(CreateSiteRequest request, CancellationToken ct)
    {
        var site = await service.CreateAsync(request, ct); return Created($"/api/sites/{site.Id}", site);
    }
}
