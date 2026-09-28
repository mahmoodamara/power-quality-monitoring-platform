using Microsoft.AspNetCore.Mvc;
using PowerQuality.Application.DTOs;
using PowerQuality.Application.Interfaces;
namespace PowerQuality.Api.Controllers;
[ApiController, Route("api/events")]
public sealed class EventsController(IEventService service) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<IReadOnlyList<PowerEventDto>>> Get([FromQuery] Guid? deviceId, [FromQuery] int limit = 100, CancellationToken ct = default) => Ok(await service.GetAsync(deviceId, limit, ct));
}
