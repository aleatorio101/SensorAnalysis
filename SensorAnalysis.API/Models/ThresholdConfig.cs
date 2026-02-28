namespace SensorAnalysis.API.Models;

public class ThresholdConfig
{
    public VariableThreshold Temperature { get; set; } = new()
    {
        Alert    = new LimitRange { Max = 30.0, Min = 15.0 },
        Critical = new LimitRange { Max = 35.0, Min = 10.0 }
    };

    public VariableThreshold Humidity { get; set; } = new()
    {
        Alert    = new LimitRange { Max = 70.0, Min = 40.0 },
        Critical = new LimitRange { Max = 80.0, Min = 30.0 }
    };

    public VariableThreshold DewPoint { get; set; } = new()
    {
        Alert    = new LimitRange { Max = 22.0 },
        Critical = new LimitRange { Max = 25.0 }
    };
}

public class VariableThreshold
{
    public LimitRange Alert { get; set; } = new();
    public LimitRange Critical { get; set; } = new();
}

public class LimitRange
{
    public double? Max { get; set; }
    public double? Min { get; set; }
}
