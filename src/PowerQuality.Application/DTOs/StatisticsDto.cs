namespace PowerQuality.Application.DTOs;
public sealed record DeviceStatisticsDto(Guid DeviceId, int Hours, int SampleCount, decimal? AverageVoltage, decimal? MinVoltage, decimal? MaxVoltage, decimal? AverageFrequency, decimal? AveragePowerFactor, int EventCount, double UptimePercent);
