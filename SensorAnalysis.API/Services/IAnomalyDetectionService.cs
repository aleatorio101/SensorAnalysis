using SensorAnalysis.API.Models;

namespace SensorAnalysis.API.Services;

public interface IAnomalyDetectionService
{
    void Fit(IReadOnlyList<SensorReading> readings);
    bool IsAnomaly(SensorReading reading);
}
