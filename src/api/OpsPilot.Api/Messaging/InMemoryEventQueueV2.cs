using System.Collections.Concurrent;
using OpsPilot.Api.Models;

namespace OpsPilot.Api.Messaging;

public class InMemoryEventQueueV2 : IEventQueueV2
{
    private readonly ConcurrentQueue<EventEnvelopeV2> _queue = new();

    public void Enqueue(EventEnvelopeV2 envelope)
    {
        if (envelope != null) _queue.Enqueue(envelope);
    }

    public bool TryDequeue(out EventEnvelopeV2? envelope)
    {
        return _queue.TryDequeue(out envelope);
    }
}