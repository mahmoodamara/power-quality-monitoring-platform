using PowerQuality.Application.Models;
using PowerQuality.Domain.Entities;
using PowerQuality.Domain.Enums;
namespace PowerQuality.Application.Rules;
public sealed class LowPowerFactorRule : IPowerQualityRule
{
    public PowerQualityRuleResult? Evaluate(Measurement m) => m.PowerFactor < 0.85m
        ? new(PowerEventType.LowPowerFactor, Severity.Warning, m.PowerFactor, $"Low power factor detected: {m.PowerFactor:F2}") : null;
}
