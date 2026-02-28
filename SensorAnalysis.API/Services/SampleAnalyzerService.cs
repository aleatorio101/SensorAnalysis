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
        var tempAnalysis     = _threshold.Analyze(reading.Temperature, _config.Temperature);
        var humidityAnalysis = _threshold.Analyze(reading.Humidity,    _config.Humidity);
        var dewPointAnalysis = _threshold.Analyze(reading.DewPoint,    _config.DewPoint);

        bool isInvalid = !reading.Temperature.HasValue
                      && !reading.Humidity.HasValue
                      && !reading.DewPoint.HasValue;

        string anomalyStatus;
        if (isInvalid)
            anomalyStatus = "invalid";
        else if (_anomaly.IsAnomaly(reading))
            anomalyStatus = "anomaly";
        else
            anomalyStatus = "normal";

        return new AnalysisResult
        {
            SensorId    = reading.SensorId,
            Type        = reading.Type,
            Timestamp   = reading.Timestamp,
            Temperature = tempAnalysis,
            Humidity    = humidityAnalysis,
            DewPoint    = dewPointAnalysis,
            Anomaly     = new AnomalyResult { Status = anomalyStatus }
        };
    }
}
