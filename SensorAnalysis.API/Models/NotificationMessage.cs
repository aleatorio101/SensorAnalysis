namespace SensorAnalysis.API.Models;

public class NotificationMessage
{
    public string SensorId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }

    public string Motivo { get; set; } = string.Empty;
}
