namespace Darp.Utils.Messaging;

/// <summary> A message source </summary>
public interface IMessageSource
{
    /// <summary> Subscribe a new sink to this source </summary>
    /// <param name="sink"> The sink to notify with new messages </param>
    /// <returns> A disposable that can be used to unsubscribe from the source </returns>
    public IDisposable Subscribe(IMessageSink sink);
}
