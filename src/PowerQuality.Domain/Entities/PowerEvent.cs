using PowerQuality.Domain.Enums;
namespace PowerQuality.Domain.Entities;
public class PowerEvent
{
    public long Id { get; set; }
    public Guid DeviceId { get; set; }
    public Device? Device { get; set; }
    public long? MeasurementId { get; set; }
    public Measurement? Measurement { get; set; }
    public PowerEventType Type { get; set; }
    public Severity Severity { get; set; }
    public decimal? Value { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
