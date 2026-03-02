using System.Threading.Channels;
using SensorAnalysis.API.Models;
using SensorAnalysis.API.Queue;
using SensorAnalysis.API.Services;

namespace SensorAnalysis.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSensorAnalysis(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ThresholdConfig>(
            configuration.GetSection("ThresholdConfig"));

        services.AddSingleton(_ => Channel.CreateUnbounded<AnalysisWorkItem>(
            new UnboundedChannelOptions { SingleReader = true }));

        services.AddSingleton<IJobRepository, InMemoryJobRepository>();

        services.AddTransient<IThresholdAnalysisService, ThresholdAnalysisService>();
        services.AddTransient<IAnomalyDetectionService, AnomalyDetectionService>();
        services.AddTransient<ISampleAnalyzerService, SampleAnalyzerService>();
        services.AddTransient<IAnalysisJobProcessor, AnalysisJobProcessor>();
        services.AddTransient<IAnalysisOrchestrator, AnalysisOrchestrator>();

        services.AddHostedService<AnalysisBackgroundService>();

        return services;
    }
}