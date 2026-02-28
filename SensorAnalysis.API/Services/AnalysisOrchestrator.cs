using SensorAnalysis.API.Models;
using SensorAnalysis.API.Queue;

namespace SensorAnalysis.API.Services;

public class AnalysisOrchestrator : IAnalysisOrchestrator
{
    private readonly IJobRepository        _jobs;
    private readonly ISampleAnalyzerService _analyzer;
    private readonly INotificationQueue    _queue;
    private readonly ILogger<AnalysisOrchestrator> _logger;

    public AnalysisOrchestrator(
        IJobRepository         jobs,
        ISampleAnalyzerService analyzer,
        INotificationQueue     queue,
        ILogger<AnalysisOrchestrator> logger)
    {
        _jobs     = jobs;
        _analyzer = analyzer;
        _queue    = queue;
        _logger   = logger;
    }

    public Task<Guid> StartAsync(IReadOnlyList<SensorReading> readings, CancellationToken ct = default)
    {
        var job = _jobs.Create();
        job.TotalSamples = readings.Count;
        _jobs.Update(job);

        _ = Task.Run(() => ProcessAsync(job.Id, readings, ct), ct);

        return Task.FromResult(job.Id);
    }

    private async Task ProcessAsync(Guid jobId, IReadOnlyList<SensorReading> readings, CancellationToken ct)
    {
        var job = _jobs.GetById(jobId)!;
        job.Status = JobStatus.Processing;
        _jobs.Update(job);

        try
        {

            _analyzer.Prepare(readings);

            var results = new List<AnalysisResult>(readings.Count);

            foreach (var reading in readings)
            {
                ct.ThrowIfCancellationRequested();

                var result = _analyzer.Analyze(reading);
                results.Add(result);

                PublishIfRequired(result);

                await Task.Yield();

                job.ProcessedSamples++;
                _jobs.Update(job);
            }

            job.Results  = results;
            job.Summary  = BuildSummary(results, _queue.DequeueAll());
            job.Status   = JobStatus.Completed;
            job.CompletedAt = DateTime.UtcNow;
            _jobs.Update(job);

            _logger.LogInformation("Job {JobId} completed. Processed {Count} samples.", jobId, results.Count);
        }
        catch (OperationCanceledException)
        {
            job.Status       = JobStatus.Failed;
            job.ErrorMessage = "Processing was cancelled.";
            _jobs.Update(job);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job {JobId} failed.", jobId);
            job.Status       = JobStatus.Failed;
            job.ErrorMessage = ex.Message;
            _jobs.Update(job);
        }
    }

    private void PublishIfRequired(AnalysisResult result)
    {
        bool isCritical = result.Temperature.Status == "critical"
                       || result.Humidity.Status    == "critical"
                       || result.DewPoint.Status    == "critical";

        bool isAnomaly = result.Anomaly.Status == "anomaly";

        if (!isCritical && !isAnomaly) return;

        _queue.Enqueue(new NotificationMessage
        {
            SensorId  = result.SensorId,
            Timestamp = result.Timestamp,
            Motivo    = isCritical ? "critical" : "anomaly"
        });
    }

    private static DashboardSummary BuildSummary(
        IReadOnlyList<AnalysisResult>   results,
        IReadOnlyList<NotificationMessage> notifications)
    {
        var summary = new DashboardSummary
        {
            TotalAnalyzed = results.Count,
            TotalInvalid  = results.Count(r => r.Anomaly.Status == "invalid"),
            TotalAnomaly  = results.Count(r => r.Anomaly.Status == "anomaly"),
            TotalNormal   = results.Count(r => r.Anomaly.Status == "normal"),

            TempAlertMaxCount    = results.Count(r => r.Temperature.Status == "alert"    && r.Temperature.LimitType == "max"),
            TempCriticalMaxCount = results.Count(r => r.Temperature.Status == "critical" && r.Temperature.LimitType == "max"),
            TempAlertMinCount    = results.Count(r => r.Temperature.Status == "alert"    && r.Temperature.LimitType == "min"),
            TempCriticalMinCount = results.Count(r => r.Temperature.Status == "critical" && r.Temperature.LimitType == "min"),

            HumidityAlertMaxCount    = results.Count(r => r.Humidity.Status == "alert"    && r.Humidity.LimitType == "max"),
            HumidityCriticalMaxCount = results.Count(r => r.Humidity.Status == "critical" && r.Humidity.LimitType == "max"),
            HumidityAlertMinCount    = results.Count(r => r.Humidity.Status == "alert"    && r.Humidity.LimitType == "min"),
            HumidityCriticalMinCount = results.Count(r => r.Humidity.Status == "critical" && r.Humidity.LimitType == "min"),

            DewPointAlertMaxCount    = results.Count(r => r.DewPoint.Status == "alert"    && r.DewPoint.LimitType == "max"),
            DewPointCriticalMaxCount = results.Count(r => r.DewPoint.Status == "critical" && r.DewPoint.LimitType == "max"),

            ByType = results
                .GroupBy(r => r.Type)
                .ToDictionary(g => g.Key, g => g.Count()),

            Notifications = [.. notifications]
        };

        return summary;
    }
}
