using FluentAssertions;
using SensorAnalysis.API.Models;
using SensorAnalysis.API.Services;

namespace SensorAnalysis.Tests;

public class ThresholdAnalysisServiceTests
{
    private readonly ThresholdAnalysisService _sut = new();

    private static readonly VariableThreshold TemperatureThreshold = new()
    {
        Alert    = new LimitRange { Max = 30.0, Min = 15.0 },
        Critical = new LimitRange { Max = 35.0, Min = 10.0 }
    };

    [Fact]
    public void Analyze_NullValue_ReturnsInvalid()
    {
        var result = _sut.Analyze(null, TemperatureThreshold);

        result.Status.Should().Be("invalid");
        result.LimitType.Should().BeNull();
        result.ThresholdValue.Should().BeNull();
    }

    [Theory]
    [InlineData(20.0, "normal")]
    public void Analyze_ValueWithinNormalRange_ReturnsNormal(double value, string expectedStatus)
    {
        var result = _sut.Analyze(value, TemperatureThreshold);
        result.Status.Should().Be(expectedStatus);
    }

    [Theory]
    [InlineData(31.0, "alert",    "max", 30.0)]
    [InlineData(14.0, "alert",    "min", 15.0)]
    [InlineData(36.0, "critical", "max", 35.0)]
    [InlineData(9.0,  "critical", "min", 10.0)]
    public void Analyze_ValueBreachesLimit_ReturnsCorrectStatus(
        double value, string expectedStatus, string expectedLimitType, double expectedThreshold)
    {
        var result = _sut.Analyze(value, TemperatureThreshold);

        result.Status.Should().Be(expectedStatus);
        result.LimitType.Should().Be(expectedLimitType);
        result.ThresholdValue.Should().Be(expectedThreshold);
    }

    [Fact]
    public void Analyze_CriticalTakesPrecedenceOverAlert()
    {
        var result = _sut.Analyze(36.0, TemperatureThreshold);
        result.Status.Should().Be("critical");
    }
}
