namespace Darp.Utils.Messaging;

/// <summary> A message sink which might receive messages of any type </summary>
public interface IAnyMessageSink : IMessageSink
{
    /// <summary> Provides the sink with a new message of any type </summary>
    /// <param name="message"> The message to be provided </param>
    /// <typeparam name="T"> The type of the message </typeparam>
    public void Publish<T>(in T message)
#if NET9_0_OR_GREATER
        where T : allows ref struct
#endif
    ;
}
