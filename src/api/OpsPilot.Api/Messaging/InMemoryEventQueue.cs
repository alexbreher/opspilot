using System.Collections.Concurrent;

namespace OpsPilot.Api.Messaging;

public class InMemoryEventQueue : IEventQueue
{
    private readonly ConcurrentQueue<object> _queue = new();

    public void Enqueue(object message)
    {
        if (message != null)
        {
            _queue.Enqueue(message);
        }
    }

    public bool TryDequeue(out object? message)
    {
        return _queue.TryDequeue(out message);
    }
}