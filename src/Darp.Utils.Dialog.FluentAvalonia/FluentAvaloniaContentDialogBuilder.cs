namespace Darp.Utils.Dialog.FluentAvalonia;

using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using DialogData;
using global::FluentAvalonia.Core;
using ContentDialogButton = ContentDialogButton;
using ContentDialogResult = Dialog.ContentDialogResult;
using FluentContentDialog = global::FluentAvalonia.UI.Controls.ContentDialog;
using FluentContentDialogButton = global::FluentAvalonia.UI.Controls.ContentDialogButton;

/// <summary> The builder based on the <see cref="FluentContentDialog"/> of FluentAvalonia </summary>
/// <typeparam name="TContent"> The type of the content </typeparam>
public sealed class FluentAvaloniaContentDialogBuilder<TContent> : IContentDialogBuilder<TContent>
{
    private readonly AvaloniaDialogService _dialogService;
    private readonly TopLevel? _topLevel;

    internal FluentContentDialog Dialog { get; }

    private CancellationTokenSource? _cancelTokenSource;

    /// <inheritdoc />
    public string Title => Dialog.Title.ToString() ?? "n/a";

    /// <inheritdoc />
    public TContent Content { get; }

    /// <summary> Initialize a new <see cref="FluentAvaloniaContentDialogBuilder{TContent}"/> </summary>
    /// <param name="dialogService"> The dialogService which created the builder </param>
    /// <param name="title"> The title of the dialog </param>
    /// <param name="content"> The content of the dialog </param>
    /// <param name="topLevel"> An optional TopLevel to show the dialog on </param>
    public FluentAvaloniaContentDialogBuilder(
        AvaloniaDialogService dialogService,
        string title,
        TContent content,
        TopLevel? topLevel = null
    )
    {
        _dialogService = dialogService;
        _topLevel = topLevel;
        Dialog = new FluentContentDialog { Title = title, Content = content };
        Dialog.Opened += (dialog, _) =>
        {
            IInputElement? firstOrDefault = dialog
                .GetLogicalDescendants()
                .OfType<IInputElement>()
                .FirstOrDefault(x => x.Focusable);
            firstOrDefault?.Focus();
        };
        Dialog.DataTemplates.Add(new DialogDataViewLocator());
        Content = content;
    }

    /// <inheritdoc />
    public IContentDialogBuilder<TContent> SetTitle(IObservable<string> observable)
    {
        EnsureNotShown();
        Dialog[!FluentContentDialog.TitleProperty] = observable.ToBinding();
        return this;
    }

    /// <inheritdoc />
    public IContentDialogBuilder<TContent> SetDefaultButton(ContentDialogButton defaultButton)
    {
        EnsureNotShown();
        Dialog.DefaultButton = (FluentContentDialogButton)defaultButton;
        return this;
    }

    /// <inheritdoc />
    public IContentDialogBuilder<TContent> SetCloseButton(string text, Func<TContent, bool>? onClick = null)
    {
        EnsureNotShown();
        Dialog.CloseButtonText = text;

        if (onClick is not null)
        {
            Dialog.CloseButtonClick += (_, args) =>
            {
                var shouldClose = onClick(Content);
                args.Cancel = !shouldClose;
            };
        }
        return this;
    }

    /// <inheritdoc />
    public IContentDialogBuilder<TContent> SetPrimaryButton(
        string text,
        IObservable<bool>? isEnabled = null,
        Func<TContent, CancellationToken, Task<bool>>? onClick = null
    )
    {
        EnsureNotShown();
        Dialog.PrimaryButtonText = text;
        BehaviorSubject<bool> notExecutingSubject = _dialogService.RegisterDisposable(new BehaviorSubject<bool>(true));
        if (isEnabled is not null)
        {
            Dialog[!FluentContentDialog.IsPrimaryButtonEnabledProperty] = isEnabled
                .CombineLatest(notExecutingSubject)
                .Select(x => x.First && x.Second)
                .ToBinding();
        }

        if (onClick is not null)
        {
            Dialog.PrimaryButtonClick += async (_, args) =>
            {
                Deferral? deferral = null;
                try
                {
                    notExecutingSubject.OnNext(false);
                    deferral = args.GetDeferral();
                    var shouldClose = await onClick(Content, _cancelTokenSource?.Token ?? default).ConfigureAwait(true);
                    args.Cancel = !shouldClose;
                }
                finally
                {
                    deferral?.Complete();
                    notExecutingSubject.OnNext(true);
                }
            };
        }
        return this;
    }

    /// <inheritdoc />
    public IContentDialogBuilder<TContent> SetSecondaryButton(
        string text,
        IObservable<bool>? isEnabled = null,
        Func<TContent, CancellationToken, Task<bool>>? onClick = null
    )
    {
        EnsureNotShown();
        Dialog.SecondaryButtonText = text;
        BehaviorSubject<bool> notExecutingSubject = _dialogService.RegisterDisposable(new BehaviorSubject<bool>(true));
        if (isEnabled is not null)
        {
            Dialog[!FluentContentDialog.IsSecondaryButtonEnabledProperty] = isEnabled
                .CombineLatest(notExecutingSubject)
                .Select(x => x.First && x.Second)
                .ToBinding();
        }

        if (onClick is not null)
        {
            Dialog.SecondaryButtonClick += async (_, args) =>
            {
                Deferral? deferral = null;
                try
                {
                    notExecutingSubject.OnNext(false);
                    deferral = args.GetDeferral();
                    var shouldClose = await onClick(Content, _cancelTokenSource?.Token ?? default).ConfigureAwait(true);
                    args.Cancel = !shouldClose;
                }
                finally
                {
                    deferral?.Complete();
                    notExecutingSubject.OnNext(true);
                }
            };
        }
        return this;
    }

    /// <inheritdoc />
    public DialogAwaitable<TContent> ShowAsync(CancellationToken cancellationToken = default)
    {
#pragma warning disable CA2000 // Call System.IDisposable.Dispose on object before all references to it are out of scope
        var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
#pragma warning restore CA2000 // Ignored because it is not going out of scope, its saved in DialogAwaitable
        Task<ContentDialogResult<TContent>> task = ShowAsync(source);
        return new DialogAwaitable<TContent>(task, source);
    }

    private async Task<ContentDialogResult<TContent>> ShowAsync(CancellationTokenSource source)
    {
        EnsureNotShown();
        _cancelTokenSource = source;
        CancellationTokenRegistration? registration = null;
        try
        {
            registration = source.Token.Register(() =>
            {
                Dispatcher.UIThread.Invoke(() => Dialog.Hide());
            });
            ContentDialogResult result;
            if (Dispatcher.UIThread.CheckAccess())
                result = await ShowDialogAsync().ConfigureAwait(true);
            else
                result = await Dispatcher.UIThread.InvokeAsync(ShowDialogAsync).ConfigureAwait(true);

            return new ContentDialogResult<TContent>(result, Content);
        }
        finally
        {
            if (registration is not null)
                await registration.Value.DisposeAsync().ConfigureAwait(true);
            if (_cancelTokenSource is not null)
                await _cancelTokenSource.CancelAsync().ConfigureAwait(true);
        }

        async Task<ContentDialogResult> ShowDialogAsync() =>
            (ContentDialogResult)await Dialog.ShowAsync(_topLevel).ConfigureAwait(true);
    }

    private void EnsureNotShown([CallerMemberName] string? operationName = null)
    {
        if (_cancelTokenSource is not null)
            throw new InvalidOperationException($"Cannot {operationName} because a dialog can only be shown once!");
    }
}
