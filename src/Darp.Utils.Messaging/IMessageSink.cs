namespace Darp.Utils.Messaging;

/// <summary> A message sink which might receive messages </summary>
#pragma warning disable CA1040 // Avoid empty interfaces
public interface IMessageSink;
#pragma warning restore CA1040

/// <summary> A message sink which might receive messages of a specific type </summary>
/// <typeparam name="T"> The type of the message to receive </typeparam>
public interface IMessageSink<T> : IMessageSink
#if NET9_0_OR_GREATER
    where T : allows ref struct
#endif
{
    /// <summary> Provides the sink with a new message </summary>
    /// <param name="message"> The message to be published </param>
    public void Publish(in T message);
}
