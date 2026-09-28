using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using PowerQuality.Application.DTOs;
namespace PowerQuality.IntegrationTests;
public sealed class ApiFlowTests : IClassFixture<PowerQualityApiFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);
    public ApiFlowTests(PowerQualityApiFactory factory)
    {
        _client = factory.CreateClient();
        _json.Converters.Add(new JsonStringEnumConverter());
    }

    [Fact]
    public async Task SeededDevices_AreAvailable()
    {
        var devices = await _client.GetFromJsonAsync<List<DeviceDto>>("/api/devices", _json);
        devices.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Telemetry_IsAccepted_AndDuplicateIsIdempotent()
    {
        var devices = await _client.GetFromJsonAsync<List<DeviceDto>>("/api/devices", _json);
        var device = devices!.First();
        var eventId = $"integration-{Guid.NewGuid():N}";
        var payload = new TelemetryIngestRequest(eventId, device.Id, 258.1m, 12.2m, 50.01m, 0.94m, 3.1m, DateTime.UtcNow);
        var first = await _client.PostAsJsonAsync("/api/telemetry", payload);
        first.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var second = await _client.PostAsJsonAsync("/api/telemetry", payload);
        second.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await second.Content.ReadFromJsonAsync<TelemetryIngestResponse>(_json);
        result!.Duplicate.Should().BeTrue();
    }
}
