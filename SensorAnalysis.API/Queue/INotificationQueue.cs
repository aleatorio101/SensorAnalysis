using SensorAnalysis.API.Models;

namespace SensorAnalysis.API.Queue;

public interface INotificationQueue
{
    void Enqueue(NotificationMessage message);
    IReadOnlyList<NotificationMessage> DequeueAll();
}
