using Microsoft.AspNetCore.Mvc;
using PowerQuality.Application.DTOs;
using PowerQuality.Application.Interfaces;
namespace PowerQuality.Api.Controllers;
[ApiController, Route("api/telemetry")]
public sealed class TelemetryController(ITelemetryService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TelemetryIngestResponse>> Ingest(TelemetryIngestRequest request, CancellationToken ct)
    {
        var correlationId = HttpContext.Items["X-Correlation-ID"]?.ToString();
        var result = await service.IngestAsync(request, correlationId, ct);
        return result.Duplicate ? Ok(result) : Accepted(result);
    }
}
