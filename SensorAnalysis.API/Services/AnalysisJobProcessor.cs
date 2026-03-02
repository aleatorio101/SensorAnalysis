using System.Collections.Concurrent;
using SensorAnalysis.API.Models;
using SensorAnalysis.API.Queue;

namespace SensorAnalysis.API.Services;

public sealed class AnalysisJobProcessor : IAnalysisJobProcessor
{
    private readonly IJobRepository                _jobs;
    private readonly ISampleAnalyzerService        _analyzer;
    private readonly INotificationQueue            _queue;
    private readonly ILogger<AnalysisJobProcessor> _logger;

    // Quantas amostras são analisadas em paralelo por job.
    // Environment.ProcessorCount aproveita todos os cores disponíveis.
    // Ajuste para um valor fixo (ex: 4) se quiser limitar o uso de CPU.
    private static readonly int Parallelism = Environment.ProcessorCount;

    public AnalysisJobProcessor(
        IJobRepository                jobs,
        ISampleAnalyzerService        analyzer,
        INotificationQueue            queue,
        ILogger<AnalysisJobProcessor> logger)
    {
        _jobs     = jobs;
        _analyzer = analyzer;
        _queue    = queue;
        _logger   = logger;
    }

    public async Task ProcessAsync(AnalysisWorkItem item, CancellationToken ct = default)
    {
        var job = _jobs.GetById(item.JobId)
            ?? throw new InvalidOperationException($"Job '{item.JobId}' not found in repository.");

        job.Status = JobStatus.Processing;
        _jobs.Update(job);

        try
        {
            // Fase 1 — Fit estatístico: precisa de todos os dados, roda sequencial.
            // É rápido (apenas cálculo de médias/percentis), não vale paralelizar.
            _analyzer.Fit(item.Readings);

            // Fase 2 — Análise paralela: cada amostra é independente após o Fit.
            var results       = new ConcurrentBag<AnalysisResult>();
            var notifications = new ConcurrentBag<NotificationMessage>();
            int processed     = 0;

            await Parallel.ForEachAsync(
                item.Readings,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = Parallelism,
                    CancellationToken      = ct
                },
                (reading, _) =>
                {
                    var result = _analyzer.Analyze(reading);
                    results.Add(result);

                    var notification = BuildNotification(result);
                    if (notification is not null)
                    {
                        _queue.Enqueue(notification);
                        notifications.Add(notification);
                    }

                    // Incremento atômico sem lock
                    var current = Interlocked.Increment(ref processed);

                    // Atualiza progresso a cada 500 amostras para não
                    // sobrecarregar o repositório com 50k writes
                    if (current % 500 == 0 || current == item.Readings.Count)
                    {
                        job.ProcessedSamples = current;
                        _jobs.Update(job);
                    }

                    return ValueTask.CompletedTask;
                });

            // Ordena pelo timestamp para o frontend exibir os gráficos corretamente
            var orderedResults = results
                .OrderBy(r => r.Timestamp)
                .ToList();

            job.Results          = orderedResults;
            job.Summary          = BuildSummary(orderedResults, notifications);
            job.ProcessedSamples = item.Readings.Count;
            job.Status           = JobStatus.Completed;
            job.CompletedAt      = DateTime.UtcNow;
            _jobs.Update(job);

            _logger.LogInformation(
                "Job {JobId} completed. Processed {Count} samples with parallelism={P}.",
                item.JobId, orderedResults.Count, Parallelism);
        }
        catch (OperationCanceledException)
        {
            job.Status       = JobStatus.Failed;
            job.ErrorMessage = "Processing was cancelled.";
            _jobs.Update(job);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job {JobId} failed.", item.JobId);
            job.Status       = JobStatus.Failed;
            job.ErrorMessage = ex.Message;
            _jobs.Update(job);
            throw;
        }
    }

    private static NotificationMessage? BuildNotification(AnalysisResult result)
    {
        bool isCritical = result.Temperature.Status == "critical"
                       || result.Humidity.Status    == "critical"
                       || result.DewPoint.Status    == "critical";

        bool isAnomaly = result.Anomaly.Status == "anomaly";

        if (!isCritical && !isAnomaly) return null;

        return new NotificationMessage
        {
            SensorId  = result.SensorId,
            Timestamp = result.Timestamp,
            Motivo    = isCritical ? "critical" : "anomaly"
        };
    }

    private static DashboardSummary BuildSummary(
        IReadOnlyList<AnalysisResult>    results,
        IEnumerable<NotificationMessage> notifications)
    {
        return new DashboardSummary
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
    }
}