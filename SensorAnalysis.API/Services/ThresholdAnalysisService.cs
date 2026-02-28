using SensorAnalysis.API.Models;

namespace SensorAnalysis.API.Services;

public class ThresholdAnalysisService : IThresholdAnalysisService
{
    public VariableAnalysis Analyze(double? value, VariableThreshold threshold)
    {
        if (value is null)
            return new VariableAnalysis { Status = "invalid" };

        var v = value.Value;

        if (threshold.Critical.Max.HasValue && v > threshold.Critical.Max.Value)
            return new VariableAnalysis
            {
                Status = "critical",
                LimitType = "max",
                ThresholdValue = threshold.Critical.Max.Value,
                Value = v
            };

        if (threshold.Critical.Min.HasValue && v < threshold.Critical.Min.Value)
            return new VariableAnalysis
            {
                Status = "critical",
                LimitType = "min",
                ThresholdValue = threshold.Critical.Min.Value,
                Value = v
            };

        if (threshold.Alert.Max.HasValue && v > threshold.Alert.Max.Value)
            return new VariableAnalysis
            {
                Status = "alert",
                LimitType = "max",
                ThresholdValue = threshold.Alert.Max.Value,
                Value = v
            };

        if (threshold.Alert.Min.HasValue && v < threshold.Alert.Min.Value)
            return new VariableAnalysis
            {
                Status = "alert",
                LimitType = "min",
                ThresholdValue = threshold.Alert.Min.Value,
                Value = v
            };

        return new VariableAnalysis { Status = "normal", Value = v };
    }
}
