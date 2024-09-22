namespace Darp.Utils.Dialog.FluentAvalonia;

using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using DialogData;
using global::FluentAvalonia.Core;
using ContentDialogButton = ContentDialogButton;
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
    public string Title { get; }
    /// <inheritdoc />
    public TContent Content { get; }

    /// <summary> Initialize a new <see cref="FluentAvaloniaContentDialogBuilder{TContent}"/> </summary>
    /// <param name="dialogService"> The dialogService which created the builder </param>
    /// <param name="title"> The title of the dialog </param>
    /// <param name="content"> The content of the dialog </param>
    /// <param name="topLevel"> An optional TopLevel to show the dialog on </param>
    public FluentAvaloniaContentDialogBuilder(AvaloniaDialogService dialogService,
        string title,
        TContent content,
        TopLevel? topLevel = null)
    {
        _dialogService = dialogService;
        _topLevel = topLevel;
        Dialog = new FluentContentDialog
        {
            Title = title,
            Content = content,
        };
        Dialog.Opened += (dialog, _) =>
        {
            IInputElement? firstOrDefault = dialog.GetLogicalDescendants()
                .OfType<IInputElement>()
                .FirstOrDefault(x => x.Focusable);
            firstOrDefault?.Focus();
        };
        Dialog.DataTemplates.Add(new DialogDataViewLocator());
        Title = title;
        Content = content;
    }

    /// <inheritdoc />
    public IContentDialogBuilder<TContent> SetDefaultButton(ContentDialogButton defaultButton)
    {
        Dialog.DefaultButton = (FluentContentDialogButton)defaultButton;
        return this;
    }

    /// <inheritdoc />
    public IContentDialogBuilder<TContent> SetCloseButton(string text,
        Func<TContent, bool>? onClick = null)
    {
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
    public IContentDialogBuilder<TContent> SetPrimaryButton(string text,
        IObservable<bool>? isEnabled = null,
        Func<TContent, CancellationToken, Task<bool>>? onClick = null)
    {
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
    public IContentDialogBuilder<TContent> SetSecondaryButton(string text,
        IObservable<bool>? isEnabled = null,
        Func<TContent, CancellationToken, Task<bool>>? onClick = null)
    {
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
    public async Task<ContentDialogResult<TContent>> ShowAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _cancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var result = (ContentDialogResult)await Dialog.ShowAsync(_topLevel).ConfigureAwait(true);
            return new ContentDialogResult<TContent>(result, Content);
        }
        finally
        {
            _cancelTokenSource?.Dispose();
            _cancelTokenSource = null;
        }
    }
}
