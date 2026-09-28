using Microsoft.AspNetCore.SignalR;
using PowerQuality.Api.Hubs;
using PowerQuality.Application.DTOs;
using PowerQuality.Application.Interfaces;
using PowerQuality.Application.Models;
namespace PowerQuality.Api.Services;
public sealed class SignalRNotifier(IHubContext<MonitoringHub> hub) : IRealtimeNotifier
{
    public Task MeasurementReceivedAsync(DeviceLiveState state, CancellationToken ct) => hub.Clients.All.SendAsync("measurementReceived", state, ct);
    public Task AlertCreatedAsync(AlertDto alert, CancellationToken ct) => hub.Clients.All.SendAsync("alertCreated", alert, ct);
    public Task DeviceStatusChangedAsync(DeviceDto device, CancellationToken ct) => hub.Clients.All.SendAsync("deviceStatusChanged", device, ct);
}
