namespace Darp.Utils.SimpleArgumentParser;

#pragma warning disable CS1591
public static class ConsoleCancellation
{
    private static readonly object SyncRoot = new();
    private static CancellationTokenSource? _cancelSource;
    private static bool _handlerRegistered;

    public static CancellationToken RegisterGracefulCancellation()
    {
        lock (SyncRoot)
        {
            if (_cancelSource is not null)
                return _cancelSource.Token;

            _cancelSource = new CancellationTokenSource();
            if (!_handlerRegistered)
            {
                Console.CancelKeyPress += OnCancelKeyPress;
                _handlerRegistered = true;
            }

            return _cancelSource.Token;
        }
    }

    private static void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        lock (SyncRoot)
        {
            if (_cancelSource is null || _cancelSource.IsCancellationRequested)
                return;

            // First Ctrl+C: cancel gracefully.
            // Second Ctrl+C: allow normal process termination.
            e.Cancel = true;
            _cancelSource.Cancel();
        }
    }
}
#pragma warning restore CS1591
