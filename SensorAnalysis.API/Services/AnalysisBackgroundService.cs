using System.Threading.Channels;
using SensorAnalysis.API.Models;
using SensorAnalysis.API.Queue;

namespace SensorAnalysis.API.Services;

public sealed class AnalysisBackgroundService : BackgroundService
{
    private readonly Channel<AnalysisWorkItem>      _channel;
    private readonly IServiceScopeFactory           _scopeFactory;
    private readonly ILogger<AnalysisBackgroundService> _logger;

    public AnalysisBackgroundService(
        Channel<AnalysisWorkItem>           channel,
        IServiceScopeFactory                scopeFactory,
        ILogger<AnalysisBackgroundService>  logger)
    {
        _channel      = channel;
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AnalysisBackgroundService started.");

        await foreach (var item in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();

                var processor = scope.ServiceProvider
                    .GetRequiredService<IAnalysisJobProcessor>();

                await processor.ProcessAsync(item, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex,
                    "Unhandled error processing job {JobId}.", item.JobId);
            }
        }

        _logger.LogInformation("AnalysisBackgroundService stopped.");
    }
}

public sealed record AnalysisWorkItem(
    Guid                          JobId,
    IReadOnlyList<SensorReading>  Readings);