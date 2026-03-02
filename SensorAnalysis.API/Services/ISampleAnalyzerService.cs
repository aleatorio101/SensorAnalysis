using SensorAnalysis.API.Models;

namespace SensorAnalysis.API.Services;

public interface ISampleAnalyzerService
{
    void Fit(IReadOnlyList<SensorReading> readings);
    AnalysisResult Analyze(SensorReading reading);
}