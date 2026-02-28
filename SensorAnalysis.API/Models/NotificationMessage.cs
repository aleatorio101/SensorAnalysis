namespace SensorAnalysis.API.Models;

public class NotificationMessage
{
    public required string SensorId { get; set; } 
    public DateTime Timestamp { get; set; }
    public string Motivo { get; set; } = string.Empty;
}
