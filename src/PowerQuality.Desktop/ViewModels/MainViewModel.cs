using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using PowerQuality.Desktop.Commands;
using PowerQuality.Desktop.Models;
using PowerQuality.Desktop.Services;
namespace PowerQuality.Desktop.ViewModels;
public sealed class MainViewModel : BaseViewModel, IAsyncDisposable
{
    private readonly ApiClient _api;
    private readonly RealtimeClient _realtime;
    private DeviceDto? _selectedDevice;
    private DeviceLiveStateDto? _liveState;
    public ObservableCollection<DeviceDto> Devices { get; } = [];
    public ObservableCollection<AlertDto> Alerts { get; } = [];
    public ICommand RefreshCommand { get; }
    public DeviceDto? SelectedDevice
    {
        get => _selectedDevice;
        set { if (Set(ref _selectedDevice, value) && value is not null) _ = LoadLiveAsync(value.Id); }
    }
    public DeviceLiveStateDto? LiveState { get => _liveState; private set => Set(ref _liveState, value); }

    public MainViewModel(ApiClient api, RealtimeClient realtime)
    {
        _api = api; _realtime = realtime; RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        _realtime.MeasurementReceived += OnMeasurementAsync;
        _realtime.AlertCreated += OnAlertAsync;
        _realtime.DeviceStatusChanged += _ => RefreshDevicesAsync();
    }

    public async Task InitializeAsync()
    {
        await RefreshAsync();
        try { await _realtime.StartAsync(); } catch { /* API may be starting; manual Refresh remains available. */ }
    }

    private async Task RefreshAsync() { await RefreshDevicesAsync(); await RefreshAlertsAsync(); if (SelectedDevice is not null) await LoadLiveAsync(SelectedDevice.Id); }
    private async Task RefreshDevicesAsync()
    {
        var current = SelectedDevice?.Id;
        var items = await _api.GetDevicesAsync();
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            Devices.Clear(); foreach (var d in items) Devices.Add(d);
            SelectedDevice = Devices.FirstOrDefault(x => x.Id == current) ?? Devices.FirstOrDefault();
        });
    }
    private async Task RefreshAlertsAsync()
    {
        var items = await _api.GetAlertsAsync();
        await Application.Current.Dispatcher.InvokeAsync(() => { Alerts.Clear(); foreach (var a in items) Alerts.Add(a); });
    }
    private async Task LoadLiveAsync(Guid id) => LiveState = await _api.GetLiveStateAsync(id);
    private async Task OnMeasurementAsync(DeviceLiveStateDto state)
    {
        if (SelectedDevice?.Id == state.DeviceId) await Application.Current.Dispatcher.InvokeAsync(() => LiveState = state);
    }
    private async Task OnAlertAsync(AlertDto alert)
    {
        await Application.Current.Dispatcher.InvokeAsync(() => Alerts.Insert(0, alert));
    }
    public async ValueTask DisposeAsync() => await _realtime.DisposeAsync();
}
