namespace Darp.Utils.SimpleArgumentParser;

public static class ConsoleCancellation
{
    private static readonly Lock SyncRoot = new();
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
                Console.CancelKeyPress += static (_, e) => OnCancelKeyPress(e);
                _handlerRegistered = true;
            }

            return _cancelSource.Token;
        }
    }

    private static void OnCancelKeyPress(ConsoleCancelEventArgs e)
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
