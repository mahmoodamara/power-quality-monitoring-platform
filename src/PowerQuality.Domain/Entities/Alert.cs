using PowerQuality.Domain.Enums;
namespace PowerQuality.Domain.Entities;
public class Alert
{
    public long Id { get; set; }
    public Guid DeviceId { get; set; }
    public Device? Device { get; set; }
    public long? PowerEventId { get; set; }
    public PowerEvent? PowerEvent { get; set; }
    public PowerEventType Type { get; set; }
    public Severity Severity { get; set; }
    public AlertStatus Status { get; set; } = AlertStatus.Active;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? AcknowledgedAtUtc { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
    public ICollection<AlertAcknowledgement> Acknowledgements { get; set; } = new List<AlertAcknowledgement>();
}
