using System.Threading.Channels;
using SensorAnalysis.API.Models;

namespace SensorAnalysis.API.Services;

public sealed class AnalysisOrchestrator : IAnalysisOrchestrator
{
    private readonly IJobRepository               _jobs;
    private readonly Channel<AnalysisWorkItem>    _channel;
    private readonly ILogger<AnalysisOrchestrator> _logger;

    public AnalysisOrchestrator(
        IJobRepository                 jobs,
        Channel<AnalysisWorkItem>      channel,
        ILogger<AnalysisOrchestrator>  logger)
    {
        _jobs    = jobs;
        _channel = channel;
        _logger  = logger;
    }

    public async Task<Guid> StartAsync(
        IReadOnlyList<SensorReading> readings,
        CancellationToken ct = default)
    {
        var job = _jobs.Create();
        job.TotalSamples = readings.Count;
        _jobs.Update(job);

        var workItem = new AnalysisWorkItem(job.Id, readings);

        await _channel.Writer.WriteAsync(workItem, ct);

        _logger.LogInformation(
            "Job {JobId} enqueued with {Count} samples.", job.Id, readings.Count);

        return job.Id;
    }
}