using PowerQuality.Domain.Enums;
namespace PowerQuality.Application.Models;
public sealed record PowerQualityRuleResult(PowerEventType Type, Severity Severity, decimal Value, string Message);
