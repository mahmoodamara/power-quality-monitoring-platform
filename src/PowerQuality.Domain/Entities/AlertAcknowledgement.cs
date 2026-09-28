namespace PowerQuality.Domain.Entities;
public class AlertAcknowledgement
{
    public long Id { get; set; }
    public long AlertId { get; set; }
    public Alert? Alert { get; set; }
    public string AcknowledgedBy { get; set; } = string.Empty;
    public string? Note { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
