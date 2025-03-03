namespace Darp.Utils.Messaging;

/// <summary> A disposable with a callback on disposal </summary>
/// <param name="state"> The state to pass to the callback </param>
/// <param name="onDispose"> The callback </param>
/// <typeparam name="TState"> The type of the state </typeparam>
public sealed class FuncDisposable<TState>(TState state, Action<TState> onDispose) : IDisposable
{
    private readonly TState _state = state;
    private readonly Action<TState> _onDispose = onDispose;

    /// <inheritdoc />
    public void Dispose() => _onDispose(_state);
}
