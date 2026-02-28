using SensorAnalysis.API.Models;

namespace SensorAnalysis.API.Services;

public interface ISampleAnalyzerService
{
    AnalysisResult Analyze(SensorReading reading);

    void Prepare(IReadOnlyList<SensorReading> allReadings);
}
