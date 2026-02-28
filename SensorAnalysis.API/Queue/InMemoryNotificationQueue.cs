using System.Collections.Concurrent;
using SensorAnalysis.API.Models;

namespace SensorAnalysis.API.Queue;

public class InMemoryNotificationQueue : INotificationQueue
{
    private readonly ConcurrentQueue<NotificationMessage> _queue = new();

    public void Enqueue(NotificationMessage message)
    {
        _queue.Enqueue(message);
    }

    public IReadOnlyList<NotificationMessage> DequeueAll()
    {
        var messages = new List<NotificationMessage>();
        while (_queue.TryDequeue(out var msg))
            messages.Add(msg);
        return messages;
    }
}
