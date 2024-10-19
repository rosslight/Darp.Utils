namespace Darp.Utils.Dialog.FluentAvalonia;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

/// <summary> The implementation of the dialog service for avalonia </summary>
public sealed class AvaloniaDialogService : IDialogService
{
    private readonly Func<object, TopLevel?> _topLevelGetter;
    private readonly List<IDisposable> _disposables = [];
    private readonly object? _dialogRoot;

    /// <summary>
    /// Initializes a new instance of <see cref="AvaloniaDialogService"/>.
    /// Tries to resolve the <see cref="TopLevel"/> from the current applications windows if multiple windows are supported.
    /// Set the dialog root using <see cref="WithDialogRoot"/> to find a <see cref="TopLevel"/> with a matching <see cref="StyledElement.DataContext"/>
    /// </summary>
    public AvaloniaDialogService()
        : this(GetCurrentApplicationWindow) { }

    /// <summary> Initializes a new instance of <see cref="AvaloniaDialogService"/> with a specified topLevelGetter </summary>
    /// <param name="topLevelGetter"> A func to resolve the top level </param>
    public AvaloniaDialogService(Func<object, TopLevel?> topLevelGetter)
        : this(null, topLevelGetter) { }

    private AvaloniaDialogService(object? dialogRoot, Func<object, TopLevel?> topLevelGetter)
    {
        _topLevelGetter = topLevelGetter;
        _dialogRoot = dialogRoot;
    }

    /// <inheritdoc />
    public IContentDialogBuilder<TContent> CreateContentDialog<TContent>(string title, TContent content)
    {
        TopLevel? topLevel = _dialogRoot is not null ? _topLevelGetter.Invoke(_dialogRoot) : null;
        return new FluentAvaloniaContentDialogBuilder<TContent>(this, title, content, topLevel);
    }

    /// <inheritdoc />
    public IDialogService WithDialogRoot(object dialogRoot) => new AvaloniaDialogService(dialogRoot, _topLevelGetter);

    internal T RegisterDisposable<T>(T disposable)
        where T : IDisposable
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

    private static Window? GetCurrentApplicationWindow(object? dialogRoot)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime applicationLifetime)
            return null;
        foreach (Window window in applicationLifetime.Windows)
        {
            if (window.DataContext == dialogRoot)
                return window;
        }
        return applicationLifetime.MainWindow;
    }
}
