namespace SensorAnalysis.API.Models;

public enum JobStatus
{
    Queued,
    Processing,
    Completed,
    Failed
}

public class ProcessingJob
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public JobStatus Status { get; set; } = JobStatus.Queued;
    public int TotalSamples { get; set; }
    public int ProcessedSamples { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public List<AnalysisResult> Results { get; set; } = [];
    public DashboardSummary? Summary { get; set; }

    public double ProgressPercent =>
        TotalSamples == 0 ? 0 : Math.Round((double)ProcessedSamples / TotalSamples * 100, 2);
}

public class DashboardSummary
{
    public int TotalAnalyzed { get; set; }
    public int TotalInvalid { get; set; }
    public int TotalNormal { get; set; }
    public int TotalAnomaly { get; set; }

    public int TempAlertMaxCount { get; set; }
    public int TempCriticalMaxCount { get; set; }
    public int TempAlertMinCount { get; set; }
    public int TempCriticalMinCount { get; set; }

    public int HumidityAlertMaxCount { get; set; }
    public int HumidityCriticalMaxCount { get; set; }
    public int HumidityAlertMinCount { get; set; }
    public int HumidityCriticalMinCount { get; set; }

    public int DewPointAlertMaxCount { get; set; }
    public int DewPointCriticalMaxCount { get; set; }

    public Dictionary<string, int> ByType { get; set; } = [];

    public List<NotificationMessage> Notifications { get; set; } = [];
}
