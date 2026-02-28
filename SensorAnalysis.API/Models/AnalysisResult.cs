namespace SensorAnalysis.API.Models;

public class AnalysisResult
{
    public string SensorId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public VariableAnalysis Temperature { get; set; } = new();
    public VariableAnalysis Humidity { get; set; } = new();
    public VariableAnalysis DewPoint { get; set; } = new();
    public AnomalyResult Anomaly { get; set; } = new();
}

public class VariableAnalysis
{

    public string Status { get; set; } = "normal";

    public string? LimitType { get; set; }

    public double? ThresholdValue { get; set; }

    public double? Value { get; set; }
}

public class AnomalyResult
{
    public string Status { get; set; } = "normal";
}
