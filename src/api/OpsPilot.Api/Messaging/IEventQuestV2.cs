using OpsPilot.Api.Models;

namespace OpsPilot.Api.Messaging;

public interface IEventQueueV2
{
    void Enqueue(EventEnvelopeV2 envelope);
    bool TryDequeue(out EventEnvelopeV2? envelope);
}