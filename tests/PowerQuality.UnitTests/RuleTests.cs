using FluentAssertions;
using PowerQuality.Application.Rules;
using PowerQuality.Domain.Entities;
using PowerQuality.Domain.Enums;
namespace PowerQuality.UnitTests;
public sealed class RuleTests
{
    [Fact]
    public void OverVoltageRule_Above250_CreatesCriticalEvent()
    {
        var result = new OverVoltageRule().Evaluate(new Measurement { Voltage = 257.4m });
        result.Should().NotBeNull(); result!.Type.Should().Be(PowerEventType.OverVoltage); result.Severity.Should().Be(Severity.Critical);
    }

    [Fact]
    public void UnderVoltageRule_NormalVoltage_ReturnsNull() => new UnderVoltageRule().Evaluate(new Measurement { Voltage = 230m }).Should().BeNull();

    [Theory]
    [InlineData(50.8, PowerEventType.HighFrequency)]
    [InlineData(49.2, PowerEventType.LowFrequency)]
    public void FrequencyDeviationRule_OutsideRange_CreatesEvent(decimal value, PowerEventType expected)
    {
        var result = new FrequencyDeviationRule().Evaluate(new Measurement { Frequency = value });
        result.Should().NotBeNull(); result!.Type.Should().Be(expected);
    }

    [Fact]
    public void LowPowerFactorRule_BelowThreshold_CreatesWarning()
    {
        var result = new LowPowerFactorRule().Evaluate(new Measurement { PowerFactor = 0.72m });
        result.Should().NotBeNull(); result!.Type.Should().Be(PowerEventType.LowPowerFactor); result.Severity.Should().Be(Severity.Warning);
    }
}
