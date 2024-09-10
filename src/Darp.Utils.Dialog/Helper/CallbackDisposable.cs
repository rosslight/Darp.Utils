namespace Darp.Utils.Dialog.Helper;

internal sealed class CallbackDisposable<T>(T state, Action<T> callback) : IDisposable
{
    private readonly T _state = state;
    private readonly Action<T> _callback = callback;

    public void Dispose() => _callback(_state);
}
