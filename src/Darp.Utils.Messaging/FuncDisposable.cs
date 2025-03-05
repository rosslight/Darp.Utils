namespace Darp.Utils.Messaging;

/// <summary> Helper methods for <see cref="FuncDisposable{TState}"/> </summary>
public static class FuncDisposable
{
    /// <summary> Create a new disposable with a state and a callback called on disposal </summary>
    /// <param name="state"> The state to pass to the callback </param>
    /// <param name="onDispose"> The callback </param>
    /// <typeparam name="TState"> The type of the state </typeparam>
    /// <returns> The disposable </returns>
    public static IDisposable Create<TState>(TState state, Action<TState> onDispose)
    {
        return new FuncDisposable<TState>(state, onDispose);
    }
}

/// <summary> A disposable with a callback on disposal </summary>
/// <param name="state"> The state to pass to the callback </param>
/// <param name="onDispose"> The callback </param>
/// <typeparam name="TState"> The type of the state </typeparam>
file sealed class FuncDisposable<TState>(TState state, Action<TState> onDispose) : IDisposable
{
    private readonly TState _state = state;
    private readonly Action<TState> _onDispose = onDispose;

    /// <inheritdoc />
    public void Dispose() => _onDispose(_state);
}
