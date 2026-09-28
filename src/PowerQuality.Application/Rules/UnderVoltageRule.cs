using PowerQuality.Application.Models;
using PowerQuality.Domain.Entities;
using PowerQuality.Domain.Enums;
namespace PowerQuality.Application.Rules;
public sealed class UnderVoltageRule : IPowerQualityRule
{
    public PowerQualityRuleResult? Evaluate(Measurement m) => m.Voltage < 210m
        ? new(PowerEventType.UnderVoltage, Severity.Warning, m.Voltage, $"Under-voltage detected: {m.Voltage:F1} V") : null;
}
