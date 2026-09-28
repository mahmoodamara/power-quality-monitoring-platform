using PowerQuality.Application.Models;
using PowerQuality.Domain.Entities;
using PowerQuality.Domain.Enums;
namespace PowerQuality.Application.Rules;
public sealed class OverVoltageRule : IPowerQualityRule
{
    public PowerQualityRuleResult? Evaluate(Measurement m) => m.Voltage > 250m
        ? new(PowerEventType.OverVoltage, Severity.Critical, m.Voltage, $"Over-voltage detected: {m.Voltage:F1} V") : null;
}
