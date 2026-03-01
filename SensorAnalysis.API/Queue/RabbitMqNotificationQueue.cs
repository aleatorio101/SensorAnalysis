using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using SensorAnalysis.API.Models;

namespace SensorAnalysis.API.Queue;

public class RabbitMqNotificationQueue : INotificationQueue, IDisposable
{
    private const string QueueName = "log_notifications";

    private readonly IConnection _connection;
    private readonly IModel      _channel;
    private readonly ILogger<RabbitMqNotificationQueue> _logger;

    public RabbitMqNotificationQueue(
        IConnection connection,
        ILogger<RabbitMqNotificationQueue> logger)
    {
        _connection = connection;
        _logger     = logger;

        _channel = _connection.CreateModel();

        _channel.QueueDeclare(
            queue:      QueueName,
            durable:    true,
            exclusive:  false,
            autoDelete: false,
            arguments:  null);
    }

    public void Enqueue(NotificationMessage message)
    {
        try
        {
            var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            var body = Encoding.UTF8.GetBytes(json);

            var props = _channel.CreateBasicProperties();
            props.Persistent   = true;
            props.ContentType  = "application/json";
            props.DeliveryMode = 2;

            _channel.BasicPublish(
                exchange:        string.Empty,
                routingKey:      QueueName,
                basicProperties: props,
                body:            body);

            _logger.LogDebug(
                "Notification published to '{Queue}': sensor={SensorId} motivo={Motivo}",
                QueueName, message.SensorId, message.Motivo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to publish notification for sensor {SensorId}.", message.SensorId);
            throw;
        }
    }

    public IReadOnlyList<NotificationMessage> DequeueAll() => [];

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}