using Microsoft.AspNetCore.Mvc;
using PowerQuality.Application.DTOs;
using PowerQuality.Application.Interfaces;
namespace PowerQuality.Api.Controllers;
[ApiController, Route("api/devices")]
public sealed class DevicesController(IDeviceService devices, ITelemetryService telemetry, IStatisticsService statistics) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<IReadOnlyList<DeviceDto>>> Get(CancellationToken ct) => Ok(await devices.GetAllAsync(ct));
    [HttpGet("{id:guid}")] public async Task<ActionResult<DeviceDto>> GetById(Guid id, CancellationToken ct) => Ok(await devices.GetByIdAsync(id, ct));
    [HttpPost] public async Task<ActionResult<DeviceDto>> Create(CreateDeviceRequest request, CancellationToken ct)
    {
        var d = await devices.CreateAsync(request, ct); return Created($"/api/devices/{d.Id}", d);
    }
    [HttpGet("{id:guid}/live")] public async Task<ActionResult<DeviceLiveStateDto>> Live(Guid id, CancellationToken ct)
    {
        var state = await devices.GetLiveStateAsync(id, ct); return state is null ? NoContent() : Ok(state);
    }
    [HttpGet("{id:guid}/measurements")] public async Task<ActionResult<IReadOnlyList<MeasurementDto>>> Measurements(Guid id, [FromQuery] DateTime? fromUtc, [FromQuery] int limit = 100, CancellationToken ct = default) => Ok(await telemetry.GetMeasurementsAsync(id, fromUtc, limit, ct));
    [HttpGet("{id:guid}/statistics")] public async Task<ActionResult<DeviceStatisticsDto>> Statistics(Guid id, [FromQuery] int hours = 24, CancellationToken ct = default) => Ok(await statistics.GetAsync(id, hours, ct));
}
