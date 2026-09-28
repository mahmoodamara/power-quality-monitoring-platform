using System.ComponentModel.DataAnnotations;
using PowerQuality.Domain.Enums;
namespace PowerQuality.Domain.Entities;
public class Device
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SiteId { get; set; }
    public Site? Site { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FirmwareVersion { get; set; } = "1.0.0";
    public DeviceStatus Status { get; set; } = DeviceStatus.Offline;
    public DateTime? LastSeenAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    [Timestamp] public byte[]? RowVersion { get; set; }
    public ICollection<Measurement> Measurements { get; set; } = new List<Measurement>();
    public ICollection<PowerEvent> PowerEvents { get; set; } = new List<PowerEvent>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}
