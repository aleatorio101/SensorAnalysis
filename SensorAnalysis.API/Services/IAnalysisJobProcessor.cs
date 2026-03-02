namespace SensorAnalysis.API.Services;

public interface IAnalysisJobProcessor
{
    Task ProcessAsync(AnalysisWorkItem item, CancellationToken ct = default);
}