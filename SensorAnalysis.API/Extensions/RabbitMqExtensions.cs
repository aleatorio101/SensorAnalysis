using RabbitMQ.Client;
using SensorAnalysis.API.Queue;

namespace SensorAnalysis.API.Extensions;

public static class RabbitMqExtensions
{
    public static IServiceCollection AddRabbitMq(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection("RabbitMq");

        services.AddSingleton<IConnection>(_ =>
        {
            var factory = new ConnectionFactory
            {
                HostName    = section["Host"]        ?? "localhost",
                Port        = int.Parse(section["Port"] ?? "5672"),
                UserName    = section["Username"]    ?? "guest",
                Password    = section["Password"]    ?? "guest",
                VirtualHost = section["VirtualHost"] ?? "/",

                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval  = TimeSpan.FromSeconds(10)
            };

            return factory.CreateConnection("sensor-analysis-producer");
        });

        services.AddSingleton<INotificationQueue, RabbitMqNotificationQueue>();

        return services;
    }
}