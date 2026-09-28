using Microsoft.EntityFrameworkCore;
using PowerQuality.Domain.Entities;
namespace PowerQuality.Infrastructure.Persistence;
public sealed class PowerQualityDbContext(DbContextOptions<PowerQualityDbContext> options) : DbContext(options)
{
    public DbSet<Site> Sites => Set<Site>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<Measurement> Measurements => Set<Measurement>();
    public DbSet<PowerEvent> PowerEvents => Set<PowerEvent>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<AlertAcknowledgement> AlertAcknowledgements => Set<AlertAcknowledgement>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Site>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(160).IsRequired();
            e.Property(x => x.Location).HasMaxLength(240);
            e.HasIndex(x => x.Name);
        });

        modelBuilder.Entity<Device>(e =>
        {
            e.Property(x => x.SerialNumber).HasMaxLength(80).IsRequired();
            e.Property(x => x.Name).HasMaxLength(160).IsRequired();
            e.Property(x => x.FirmwareVersion).HasMaxLength(40);
            e.HasIndex(x => x.SerialNumber).IsUnique();
            e.HasIndex(x => new { x.SiteId, x.Status });
            e.HasOne(x => x.Site).WithMany(x => x.Devices).HasForeignKey(x => x.SiteId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Measurement>(e =>
        {
            e.Property(x => x.ExternalEventId).HasMaxLength(120).IsRequired();
            e.Property(x => x.Voltage).HasPrecision(10, 3);
            e.Property(x => x.Current).HasPrecision(12, 3);
            e.Property(x => x.Frequency).HasPrecision(8, 4);
            e.Property(x => x.PowerFactor).HasPrecision(6, 4);
            e.Property(x => x.ActivePower).HasPrecision(14, 4);
            e.HasIndex(x => x.ExternalEventId).IsUnique();
            e.HasIndex(x => new { x.DeviceId, x.TimestampUtc }).IsDescending(false, true);
            e.HasIndex(x => x.ProcessedAtUtc);
            e.HasOne(x => x.Device).WithMany(x => x.Measurements).HasForeignKey(x => x.DeviceId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PowerEvent>(e =>
        {
            e.Property(x => x.Message).HasMaxLength(500).IsRequired();
            e.Property(x => x.Value).HasPrecision(14, 4);
            e.HasIndex(x => new { x.DeviceId, x.CreatedAtUtc }).IsDescending(false, true);
            e.HasOne(x => x.Device).WithMany(x => x.PowerEvents).HasForeignKey(x => x.DeviceId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Measurement).WithMany().HasForeignKey(x => x.MeasurementId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Alert>(e =>
        {
            e.Property(x => x.Message).HasMaxLength(500).IsRequired();
            e.HasIndex(x => new { x.Status, x.CreatedAtUtc }).IsDescending(false, true);
            e.HasIndex(x => new { x.DeviceId, x.Type, x.Status });
            e.HasOne(x => x.Device).WithMany(x => x.Alerts).HasForeignKey(x => x.DeviceId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.PowerEvent).WithMany().HasForeignKey(x => x.PowerEventId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<AlertAcknowledgement>(e =>
        {
            e.Property(x => x.AcknowledgedBy).HasMaxLength(120).IsRequired();
            e.Property(x => x.Note).HasMaxLength(500);
            e.HasOne(x => x.Alert).WithMany(x => x.Acknowledgements).HasForeignKey(x => x.AlertId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuditLog>(e =>
        {
            e.Property(x => x.Action).HasMaxLength(120).IsRequired();
            e.Property(x => x.EntityType).HasMaxLength(120).IsRequired();
            e.Property(x => x.EntityId).HasMaxLength(120).IsRequired();
            e.Property(x => x.CorrelationId).HasMaxLength(120);
            e.HasIndex(x => x.CreatedAtUtc).IsDescending();
        });
    }
}
