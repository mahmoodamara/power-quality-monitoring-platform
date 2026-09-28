namespace PowerQuality.Domain.Entities;
public class Measurement
{
    public long Id { get; set; }
    public string ExternalEventId { get; set; } = string.Empty;
    public Guid DeviceId { get; set; }
    public Device? Device { get; set; }
    public decimal Voltage { get; set; }
    public decimal Current { get; set; }
    public decimal Frequency { get; set; }
    public decimal PowerFactor { get; set; }
    public decimal ActivePower { get; set; }
    public DateTime TimestampUtc { get; set; }
    public DateTime ReceivedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAtUtc { get; set; }
}
