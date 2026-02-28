using SensorAnalysis.API.Models;

namespace SensorAnalysis.API.Services;

public interface IThresholdAnalysisService
{
    VariableAnalysis Analyze(double? value, VariableThreshold threshold);
}
