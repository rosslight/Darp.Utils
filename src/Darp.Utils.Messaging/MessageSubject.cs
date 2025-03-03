namespace Darp.Utils.Messaging;

#if NET9_0_OR_GREATER
using Lock = System.Threading.Lock;
#else
using Lock = object;
#endif

/// <summary> A default message source </summary>
public sealed class MessageSubject : IMessageSource, IAnyMessageSink
{
    private readonly List<IMessageSink> _eventReceiverProxies = [];

    private readonly Lock _lock = new();

    /// <inheritdoc />
    public void Publish<T>(in T message)
#if NET9_0_OR_GREATER
        where T : allows ref struct
#endif
    {
        lock (_lock)
        {
            foreach (IMessageSink eventReceiver in _eventReceiverProxies)
            {
                if (eventReceiver is IMessageSink<T> receiver)
                    receiver.Publish(message);
                else if (eventReceiver is IAnyMessageSink anyReceiver)
                    anyReceiver.Publish(message);
            }
        }
    }

    /// <inheritdoc />
    public IDisposable Subscribe(IMessageSink sink)
    {
        lock (_lock)
        {
            _eventReceiverProxies.Add(sink);
            return new FuncDisposable<(Lock Lock, List<IMessageSink> Sinks, IMessageSink Sink)>(
                (_lock, _eventReceiverProxies, sink),
                state =>
                {
                    lock (state.Lock)
                        state.Sinks.Remove(state.Sink);
                }
            );
        }
    }
}
