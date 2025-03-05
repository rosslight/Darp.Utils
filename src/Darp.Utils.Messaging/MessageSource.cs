namespace Darp.Utils.Messaging;

using System.Runtime.CompilerServices;

/// <summary> Extensions for a message source </summary>
public static class MessageSource
{
    /// <summary> Subscribe a provider to a message source </summary>
    /// <param name="source"> The source to subscribe to </param>
    /// <param name="provider"> The provider to provide the sink </param>
    /// <typeparam name="TProvider"> The type of the provider </typeparam>
    /// <returns> A disposable that can be used to unsubscribe from the source </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable Subscribe<TProvider>(this IMessageSource source, TProvider provider)
        where TProvider : IMessageSinkProvider
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(source);
#else
        if (source is null)
            throw new ArgumentNullException(nameof(source));
#endif
        return source.Subscribe(provider.GetMessageSink());
    }

    /// <summary> Subscribe a callback to a message source </summary>
    /// <param name="source"> The source to subscribe to </param>
    /// <param name="onMessage"> The callback to be called when a message of the specified type is received </param>
    /// <typeparam name="T"> The type of the message to listen for </typeparam>
    /// <returns> A disposable that can be used to unsubscribe from the source </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable Subscribe<T>(this IMessageSource source, Action<T> onMessage)
#if NET9_0_OR_GREATER
        where T : allows ref struct
#endif
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(source);
#else
        if (source is null)
            throw new ArgumentNullException(nameof(source));
#endif
        return source.Subscribe(new ActionMessageSink<T>(onMessage));
    }

    /// <summary> Convert the message source to an observable </summary>
    /// <param name="source"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IObservable<T> AsObservable<T>(this IMessageSource source)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(source);
#else
        if (source is null)
            throw new ArgumentNullException(nameof(source));
#endif
        return new ObservableMessageSink<T>(source);
    }
}

file sealed class ActionMessageSink<T>(Action<T> action) : IMessageSink<T>
#if NET9_0_OR_GREATER
    where T : allows ref struct
#endif
{
    private readonly Action<T> _action = action;

    public void Publish(in T message) => _action(message);
}

file sealed class ActionStateMessageSink<TState, T>(TState state, Action<TState, T> action) : IMessageSink<T>
#if NET9_0_OR_GREATER
    where T : allows ref struct
#endif
{
    private readonly TState _state = state;
    private readonly Action<TState, T> _action = action;

    public void Publish(in T message) => _action(_state, message);
}

file sealed class ObservableMessageSink<T>(IMessageSource source) : IObservable<T>
{
    private readonly IMessageSource _source = source;

    /// <inheritdoc />
    public IDisposable Subscribe(IObserver<T> sink) =>
        _source.Subscribe(new ActionStateMessageSink<IObserver<T>, T>(sink, (state, message) => state.OnNext(message)));
}
