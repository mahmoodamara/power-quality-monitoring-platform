namespace PowerQuality.Application.Interfaces;
public interface IOfflineDetectionService
{
    Task<int> DetectAsync(TimeSpan offlineAfter, CancellationToken cancellationToken);
}
