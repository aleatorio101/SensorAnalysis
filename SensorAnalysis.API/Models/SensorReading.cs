namespace SensorAnalysis.API.Models;

public class SensorReading
{
    public required string sensor_id { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime timestamp { get; set; }
    public double? temperature { get; set; }
    public double? humidity { get; set; }
    public double? dew_point { get; set; }
}
