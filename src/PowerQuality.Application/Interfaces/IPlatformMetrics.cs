namespace PowerQuality.Application.Interfaces;
public interface IPlatformMetrics
{
    void TelemetryReceived();
    void TelemetryRejected();
    void AlertCreated();
}
