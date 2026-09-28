using PowerQuality.Application.Models;
using PowerQuality.Domain.Entities;
using PowerQuality.Domain.Enums;
namespace PowerQuality.Application.Rules;
public sealed class FrequencyDeviationRule : IPowerQualityRule
{
    public PowerQualityRuleResult? Evaluate(Measurement m)
    {
        if (m.Frequency > 50.5m) return new(PowerEventType.HighFrequency, Severity.Critical, m.Frequency, $"High frequency detected: {m.Frequency:F2} Hz");
        if (m.Frequency < 49.5m) return new(PowerEventType.LowFrequency, Severity.Critical, m.Frequency, $"Low frequency detected: {m.Frequency:F2} Hz");
        return null;
    }
}
