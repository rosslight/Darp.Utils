namespace Darp.Utils.SimpleArgumentParser;

/// <summary>
/// Creates a cancellation token that is cancelled by Ctrl+C or process exit.
/// </summary>
public static class ConsoleCancellation
{
    private static readonly Lock SyncRoot = new();
    private static CancellationTokenSource? _cancelSource;
    private static bool _handlerRegistered;

    /// <summary>
    /// Registers console shutdown handlers and returns the shared cancellation token.
    /// </summary>
    /// <returns>A token that is cancelled when the process is asked to stop.</returns>
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
