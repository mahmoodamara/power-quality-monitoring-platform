using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PowerQuality.Application.DTOs;
using PowerQuality.Application.Interfaces;
namespace PowerQuality.Infrastructure.Services;
public sealed class WebhookNotificationClient(HttpClient http, IConfiguration configuration, ILogger<WebhookNotificationClient> logger) : INotificationClient
{
    public async Task SendAlertAsync(AlertDto alert, CancellationToken ct)
    {
        var url = configuration["Notifications:WebhookUrl"];
        if (string.IsNullOrWhiteSpace(url)) return;
        try
        {
            using var response = await http.PostAsJsonAsync(url, alert, ct);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex) { logger.LogWarning(ex, "Alert notification delivery failed for alert {AlertId}", alert.Id); }
    }
}
