namespace Darp.Utils.Dialog;

using System.Runtime.CompilerServices;

/// <summary> An awaitable and disposable class representing an open dialog </summary>
/// <param name="dialogTask"> The task to await </param>
/// <param name="closeDialogSource"> The cancellation token source to allow cancellation </param>
#pragma warning disable CA1815 // Override equals and operator equals on value types
public readonly struct DialogAwaitable<TContent>(
    Task<ContentDialogResult<TContent>> dialogTask,
    CancellationTokenSource closeDialogSource
) : IDisposable
#pragma warning restore CA1815
{
    private readonly Task<ContentDialogResult<TContent>> _dialogTask = dialogTask;
    private readonly CancellationTokenSource _closeDialogSource = closeDialogSource;

    /// <summary> Gets the <see cref="CancellationToken"/> which notifies when the dialog has closed </summary>
    public CancellationToken Token { get; } = closeDialogSource.Token;

    /// <summary> <c>False</c>, if the dialog is still showing, <c>true</c> otherwise </summary>
    public bool IsClosed => _dialogTask.IsCompleted || Token.IsCancellationRequested;

    /// <summary> Gets an awaiter used to await this <see cref="DialogAwaitable{TContent}"/>. </summary>
    /// <returns> An awaiter instance. </returns>
    public TaskAwaiter<ContentDialogResult<TContent>> GetAwaiter() => _dialogTask.GetAwaiter();

    /// <summary> Disposes of the current dialog. If still open, the dialog will be closed </summary>
    public void Dispose()
    {
        try
        {
            _closeDialogSource.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // The dialog task owns disposal of the linked source and may already have completed.
        }
    }
}
