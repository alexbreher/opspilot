using System.Threading.Channels;
using OpsPilot.Api.Models;

namespace OpsPilot.Api.Messaging;

public class ChannelBackgroundEventQueue : IBackgroundEventQueue
{
    private readonly Channel<EventEnvelopeV2> _channel;

    public ChannelBackgroundEventQueue()
    {
        // Unbounded is fine for dev; later we can tune bounded capacity.
        _channel = Channel.CreateUnbounded<EventEnvelopeV2>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    }

    public ValueTask EnqueueAsync(EventEnvelopeV2 envelope, CancellationToken ct = default)
    {
        if (envelope == null) return ValueTask.CompletedTask;
        return _channel.Writer.WriteAsync(envelope, ct);
    }

    public async IAsyncEnumerable<EventEnvelopeV2> DequeueAllAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        while (await _channel.Reader.WaitToReadAsync(ct))
        {
            while (_channel.Reader.TryRead(out var item))
            {
                yield return item;
            }
        }
    }
}