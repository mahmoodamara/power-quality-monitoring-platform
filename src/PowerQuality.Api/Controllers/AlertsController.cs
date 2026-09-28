using Microsoft.AspNetCore.Mvc;
using PowerQuality.Application.DTOs;
using PowerQuality.Application.Interfaces;
using PowerQuality.Domain.Enums;
namespace PowerQuality.Api.Controllers;
[ApiController, Route("api/alerts")]
public sealed class AlertsController(IAlertService service) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<IReadOnlyList<AlertDto>>> Get([FromQuery] AlertStatus? status, [FromQuery] int limit = 100, CancellationToken ct = default) => Ok(await service.GetAsync(status, limit, ct));
    [HttpPost("{id:long}/acknowledge")] public async Task<ActionResult<AlertDto>> Acknowledge(long id, AcknowledgeAlertRequest request, CancellationToken ct)
    {
        var correlationId = HttpContext.Items["X-Correlation-ID"]?.ToString(); return Ok(await service.AcknowledgeAsync(id, request, correlationId, ct));
    }
}
