namespace Darp.Utils.Messaging;

/// <summary> A provider for a message sink </summary>
public interface IMessageSinkProvider
{
    /// <summary> Get a new message sink </summary>
    /// <returns> The <see cref="IMessageSink"/> that can be subscribed to a <see cref="IMessageSource"/> </returns>
    public IMessageSink GetMessageSink();
}
