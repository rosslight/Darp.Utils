namespace Darp.Utils.Messaging;

using System.Collections.Immutable;

/// <summary> A default message source </summary>
public sealed class MessageSubject : IMessageSource, IAnyMessageSink
{
    private ImmutableArray<IMessageSink> _eventReceiverProxies = [];

    /// <inheritdoc />
    public void Publish<T>(in T message)
#if NET9_0_OR_GREATER
        where T : allows ref struct
#endif
    {
        foreach (IMessageSink eventReceiver in _eventReceiverProxies)
        {
            if (eventReceiver is IMessageSink<T> receiver)
                receiver.Publish(message);
            else if (eventReceiver is IAnyMessageSink anyReceiver)
                anyReceiver.Publish(message);
        }
    }

    /// <inheritdoc />
    public IDisposable Subscribe(IMessageSink sink)
    {
        _eventReceiverProxies = _eventReceiverProxies.Add(sink);
        return FuncDisposable.Create<(MessageSubject Self, IMessageSink Sink)>(
            (this, sink),
            state =>
            {
                state.Self._eventReceiverProxies = state.Self._eventReceiverProxies.Remove(state.Sink);
            }
        );
    }
}
