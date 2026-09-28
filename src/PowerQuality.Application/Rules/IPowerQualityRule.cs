using PowerQuality.Application.Models;
using PowerQuality.Domain.Entities;
namespace PowerQuality.Application.Rules;
public interface IPowerQualityRule { PowerQualityRuleResult? Evaluate(Measurement measurement); }
