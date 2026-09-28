using System.Net.Http.Json;
using System.Text.Json;
using PowerQuality.Desktop.Models;
namespace PowerQuality.Desktop.Services;
public sealed class ApiClient
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };
    public ApiClient(string baseUrl) => _http = new HttpClient { BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/") };
    public async Task<IReadOnlyList<DeviceDto>> GetDevicesAsync(CancellationToken ct = default) => await _http.GetFromJsonAsync<List<DeviceDto>>("api/devices", _json, ct) ?? [];
    public async Task<IReadOnlyList<AlertDto>> GetAlertsAsync(CancellationToken ct = default) => await _http.GetFromJsonAsync<List<AlertDto>>("api/alerts?limit=100", _json, ct) ?? [];
    public async Task<DeviceLiveStateDto?> GetLiveStateAsync(Guid deviceId, CancellationToken ct = default)
    {
        using var response = await _http.GetAsync($"api/devices/{deviceId}/live", ct);
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DeviceLiveStateDto>(_json, ct);
    }
}
