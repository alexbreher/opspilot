using OpsPilot.Api.Models;

namespace OpsPilot.Api.Messaging;

public interface IBackgroundEventQueue
{
    ValueTask EnqueueAsync(EventEnvelopeV2 envelope, CancellationToken ct = default);
    IAsyncEnumerable<EventEnvelopeV2> DequeueAllAsync(CancellationToken ct);
}