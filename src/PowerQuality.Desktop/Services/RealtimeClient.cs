using Microsoft.AspNetCore.SignalR.Client;
using PowerQuality.Desktop.Models;
namespace PowerQuality.Desktop.Services;
public sealed class RealtimeClient : IAsyncDisposable
{
    private readonly HubConnection _connection;
    public event Func<DeviceLiveStateDto, Task>? MeasurementReceived;
    public event Func<AlertDto, Task>? AlertCreated;
    public event Func<DeviceDto, Task>? DeviceStatusChanged;
    public RealtimeClient(string hubUrl)
    {
        _connection = new HubConnectionBuilder().WithUrl(hubUrl).WithAutomaticReconnect().Build();
        _connection.On<DeviceLiveStateDto>("measurementReceived", async state => { if (MeasurementReceived is not null) await MeasurementReceived(state); });
        _connection.On<AlertDto>("alertCreated", async alert => { if (AlertCreated is not null) await AlertCreated(alert); });
        _connection.On<DeviceDto>("deviceStatusChanged", async device => { if (DeviceStatusChanged is not null) await DeviceStatusChanged(device); });
    }
    public Task StartAsync(CancellationToken ct = default) => _connection.StartAsync(ct);
    public async ValueTask DisposeAsync() => await _connection.DisposeAsync();
}
