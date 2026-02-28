using FluentAssertions;
using SensorAnalysis.API.Models;
using SensorAnalysis.API.Services;

namespace SensorAnalysis.Tests;

public class AnomalyDetectionServiceTests
{
    private readonly AnomalyDetectionService _sut = new();

    private static SensorReading MakeReading(
        string sensorId = "s1",
        double temp = 25.0,
        double humidity = 55.0,
        double dew = 18.0) =>
        new()
        {
            SensorId    = sensorId,
            Type        = "FM5308",
            Timestamp   = DateTime.UtcNow,
            Temperature = temp,
            Humidity    = humidity,
            DewPoint    = dew
        };

    [Fact]
    public void IsAnomaly_NullReading_ReturnsFalse()
    {
        var readings = Enumerable.Range(1, 20).Select(_ => MakeReading()).ToList();
        _sut.Fit(readings);

        var nullReading = new SensorReading
        {
            SensorId = "s1", Type = "FM5308",
            Timestamp = DateTime.UtcNow,
            Temperature = null, Humidity = null, DewPoint = null
        };

        _sut.IsAnomaly(nullReading).Should().BeFalse();
    }

    [Fact]
    public void IsAnomaly_NormalReading_ReturnsFalse()
    {
        var normal = Enumerable.Range(1, 50).Select(_ => MakeReading()).ToList();
        _sut.Fit(normal);

        _sut.IsAnomaly(MakeReading()).Should().BeFalse();
    }

    [Fact]
    public void IsAnomaly_ExtremeOutlier_ReturnsTrue()
    {
        var readings = Enumerable.Range(1, 50).Select(_ => MakeReading()).ToList();
        _sut.Fit(readings);

        var outlier = MakeReading(temp: 999.0, humidity: 999.0, dew: 999.0);
        _sut.IsAnomaly(outlier).Should().BeTrue();
    }

    [Fact]
    public void IsAnomaly_StaleSensor_ReturnsTrue()
    {
        var readings = Enumerable.Range(1, 10)
            .Select(_ => MakeReading(sensorId: "s1", humidity: 65.0))
            .ToList();

        _sut.Fit(readings);

        _sut.IsAnomaly(MakeReading(sensorId: "s1")).Should().BeTrue();
    }
}
