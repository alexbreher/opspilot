using System.Collections.Concurrent;

namespace OpsPilot.Api.Messaging;

public class InMemoryEventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();
    private readonly IEventQueue _queue;

    public InMemoryEventBus(IEventQueue queue)
    {
        _queue = queue;
    }

    public void Publish<T>(T @event) where T : class
    {
        if (@event == null) return;

        _queue.Enqueue(@event);

        if (_handlers.TryGetValue(typeof(T), out var list))
        {
            // copy to avoid mutation during enumeration
            var handlers = list.ToArray();
            foreach (var d in handlers)
            {
                if (d is Action<T> action)
                {
                    action(@event);
                }
            }
        }
    }

    public void Subscribe<T>(Action<T> handler) where T : class
    {
        var key = typeof(T);
        _handlers.AddOrUpdate(key, _ => new List<Delegate> 
        { handler},
        (_, existing) => {
            existing.Add(handler);
            return existing;
            });
    }
}