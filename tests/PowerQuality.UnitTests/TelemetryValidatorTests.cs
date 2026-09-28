using FluentAssertions;
using PowerQuality.Application.DTOs;
using PowerQuality.Application.Exceptions;
using PowerQuality.Application.Validation;
namespace PowerQuality.UnitTests;
public sealed class TelemetryValidatorTests
{
    [Fact]
    public void ValidTelemetry_DoesNotThrow()
    {
        var request = new TelemetryIngestRequest("evt-1", Guid.NewGuid(), 230m, 10m, 50m, 0.95m, 2.3m, DateTime.UtcNow);
        var act = () => TelemetryValidator.Validate(request);
        act.Should().NotThrow();
    }

    [Fact]
    public void InvalidPowerFactor_ThrowsValidationException()
    {
        var request = new TelemetryIngestRequest("evt-1", Guid.NewGuid(), 230m, 10m, 50m, 1.2m, 2.3m, DateTime.UtcNow);
        var act = () => TelemetryValidator.Validate(request);
        act.Should().Throw<ValidationException>().Which.Code.Should().Be("INVALID_POWER_FACTOR");
    }
}
