namespace Darp.Utils.Dialog;

/// <summary> The implementation of the dialog service for avalonia </summary>
public sealed class AvaloniaDialogService : IDialogService
{
    private readonly List<IDisposable> _disposables = [];

    /// <inheritdoc />
    public IContentDialogBuilder<TContent> CreateContentDialog<TContent>(string title, TContent content) =>
        new FluentAvaloniaContentDialogBuilder<TContent>(this, title, content);

    internal T RegisterDisposable<T>(T disposable) where T : IDisposable
    {
        _disposables.Add(disposable);
        return disposable;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (IDisposable disposable in _disposables)
        {
            disposable.Dispose();
        }
        _disposables.Clear();
    }
}
