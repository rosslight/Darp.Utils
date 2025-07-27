namespace Darp.Utils.Dialog;

using System.Runtime.CompilerServices;

/// <summary> An awaitable and disposable class representing an open dialog </summary>
#pragma warning disable CA1815 // Override equals and operator equals on value types
public readonly struct DialogAwaitable<TContent> : IDisposable
#pragma warning restore CA1815
{
    private readonly Task<ContentDialogResult<TContent>> _dialogTask;
    private readonly CancellationTokenSource _closeDialogSource;

    /// <summary> Initializes a new <see cref="DialogAwaitable{TContent}"/> </summary>
    /// <param name="task"> The task to await </param>
    /// <param name="source"> The cancellation token source to allow cancellation </param>
    public DialogAwaitable(Task<ContentDialogResult<TContent>> task, CancellationTokenSource source)
    {
        _dialogTask = task;
        _closeDialogSource = source;
    }

    /// <summary> Gets the <see cref="CancellationToken"/> which notifies when the dialog has closed </summary>
    public CancellationToken Token => _closeDialogSource.Token;

    /// <summary> <c>False</c>, if the dialog is still showing, <c>true</c> otherwise </summary>
    public bool IsClosed => _closeDialogSource.Token.IsCancellationRequested;

    /// <summary> Gets an awaiter used to await this <see cref="DialogAwaitable{TContent}"/>. </summary>
    /// <returns> An awaiter instance. </returns>
    public TaskAwaiter<ContentDialogResult<TContent>> GetAwaiter() => _dialogTask.GetAwaiter();

    /// <summary> Disposes of the current dialog. If still open, the dialog will be closed </summary>
    public void Dispose()
    {
        _closeDialogSource.Cancel();
        _closeDialogSource.Dispose();
    }
}
