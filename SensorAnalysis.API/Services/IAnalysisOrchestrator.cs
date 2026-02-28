using SensorAnalysis.API.Models;

namespace SensorAnalysis.API.Services;

public interface IAnalysisOrchestrator
{
    Task<Guid> StartAsync(IReadOnlyList<SensorReading> readings, CancellationToken ct = default);
}
