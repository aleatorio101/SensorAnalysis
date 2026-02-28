using SensorAnalysis.API.Models;
using Microsoft.Extensions.Options;

namespace SensorAnalysis.API.Services;

public class SampleAnalyzerService : ISampleAnalyzerService
{
    private readonly IThresholdAnalysisService _threshold;
    private readonly IAnomalyDetectionService  _anomaly;
    private readonly ThresholdConfig           _config;

    public SampleAnalyzerService(
        IThresholdAnalysisService threshold,
        IAnomalyDetectionService  anomaly,
        IOptions<ThresholdConfig> config)
    {
        _threshold = threshold;
        _anomaly   = anomaly;
        _config    = config.Value;
    }

    public void Prepare(IReadOnlyList<SensorReading> allReadings) =>
        _anomaly.Fit(allReadings);

    public AnalysisResult Analyze(SensorReading reading)
    {
        var tempAnalysis     = _threshold.Analyze(reading.temperature, _config.Temperature);
        var humidityAnalysis = _threshold.Analyze(reading.humidity,    _config.Humidity);
        var dewPointAnalysis = _threshold.Analyze(reading.dew_point,    _config.DewPoint);

        bool isInvalid = !reading.temperature.HasValue
                      && !reading.humidity.HasValue
                      && !reading.dew_point.HasValue;

        string anomalyStatus;
        if (isInvalid)
            anomalyStatus = "invalid";
        else if (_anomaly.IsAnomaly(reading))
            anomalyStatus = "anomaly";
        else
            anomalyStatus = "normal";

        return new AnalysisResult
        {
            SensorId    = reading.sensor_id,
            Type        = reading.Type,
            Timestamp   = reading.timestamp,
            Temperature = tempAnalysis,
            Humidity    = humidityAnalysis,
            DewPoint    = dewPointAnalysis,
            Anomaly     = new AnomalyResult { Status = anomalyStatus }
        };
    }
}
