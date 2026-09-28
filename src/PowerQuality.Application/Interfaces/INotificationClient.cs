using PowerQuality.Application.DTOs;
namespace PowerQuality.Application.Interfaces;
public interface INotificationClient
{
    Task SendAlertAsync(AlertDto alert, CancellationToken cancellationToken);
}
