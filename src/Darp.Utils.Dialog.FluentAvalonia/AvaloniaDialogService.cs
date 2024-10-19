namespace Darp.Utils.Dialog.FluentAvalonia;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

/// <summary> The implementation of the dialog service for avalonia </summary>
public sealed class AvaloniaDialogService : IDialogService
{
    private readonly Func<Type, TopLevel?> _topLevelGetter;
    private readonly List<IDisposable> _disposables = [];
    private readonly Type? _dialogRootType;

    /// <summary>
    /// Initializes a new instance of <see cref="AvaloniaDialogService"/>.
    /// Tries to resolve the <see cref="TopLevel"/> from the current applications windows if multiple windows are supported.
    /// Set the dialog root type using <see cref="WithDialogRoot"/> to find a <see cref="TopLevel"/> with a matching <see cref="StyledElement.DataContext"/>
    /// </summary>
    public AvaloniaDialogService()
        : this(GetCurrentApplicationWindow) { }

    /// <summary> Initializes a new instance of <see cref="AvaloniaDialogService"/> with a specified topLevelGetter </summary>
    /// <param name="topLevelGetter"> A func to resolve the top level </param>
    public AvaloniaDialogService(Func<Type, TopLevel?> topLevelGetter)
        : this(null, topLevelGetter) { }

    private AvaloniaDialogService(Type? dialogRootType, Func<Type, TopLevel?> topLevelGetter)
    {
        _topLevelGetter = topLevelGetter;
        _dialogRootType = dialogRootType;
    }

    /// <inheritdoc />
    public IContentDialogBuilder<TContent> CreateContentDialog<TContent>(string title, TContent content)
    {
        TopLevel? topLevel = _dialogRootType is not null ? _topLevelGetter.Invoke(_dialogRootType) : null;
        return new FluentAvaloniaContentDialogBuilder<TContent>(this, title, content, topLevel);
    }

    /// <inheritdoc />
    public IDialogService WithDialogRoot(Type dialogRootType)
    {
        return dialogRootType == _dialogRootType ? this : new AvaloniaDialogService(dialogRootType, _topLevelGetter);
    }

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

    private static Window? GetCurrentApplicationWindow(Type? dialogRootType)
    {
        if (dialogRootType is null)
            return null;
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime applicationLifetime)
            return null;
        foreach (Window window in applicationLifetime.Windows)
        {
            if (window.DataContext?.GetType() == dialogRootType)
                return window;
        }
        return applicationLifetime.MainWindow;
    }
}
