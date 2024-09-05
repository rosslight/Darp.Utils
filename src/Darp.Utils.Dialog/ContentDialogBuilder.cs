namespace Darp.Utils.Dialog;

using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;

/// <summary> The builder based on the <see cref="ContentDialog"/> of FluentAvalonia </summary>
/// <typeparam name="TContent"> The type of the content </typeparam>
public sealed class ContentDialogBuilder<TContent>
{
    private readonly AvaloniaDialogService _dialogService;
    private readonly ContentDialog _dialog;

    private CancellationTokenSource? _cancelTokenSource;
    /// <summary> The content of the dialog </summary>
    public TContent Content { get; }

    /// <summary> Initialize a new <see cref="ContentDialogBuilder{TContent}"/> </summary>
    /// <param name="dialogService"> The dialogService which created the builder </param>
    /// <param name="title"> The title of the dialog </param>
    /// <param name="content"> The content of the dialog </param>
    public ContentDialogBuilder(AvaloniaDialogService dialogService, string title, TContent content)
    {
        _dialogService = dialogService;
        _dialog = new ContentDialog
        {
            Title = title,
            Content = content,
        };
        _dialog.Opened += (dialog, _) =>
        {
            IInputElement? firstOrDefault = dialog.GetLogicalDescendants()
                .OfType<IInputElement>()
                .FirstOrDefault(x => x.Focusable);
            firstOrDefault?.Focus();
        };
        _dialog.DataTemplates.Add(new FuncDataTemplate<MessageBoxModel>((model, _) => model.IsSelectable
            ? new SelectableTextBlock { Text = model.Message, TextWrapping = TextWrapping.Wrap }
            : new TextBlock { Text = model.Message, TextWrapping = TextWrapping.Wrap }));
        Content = content;
    }

    /// <summary> Sets the button which is shown as the default button </summary>
    /// <param name="defaultButton"> The default <see cref="ContentDialogButton"/> </param>
    /// <returns> The same <see cref="ContentDialogBuilder{TContent}"/> instance </returns>
    public ContentDialogBuilder<TContent> SetDefaultButton(ContentDialogButton defaultButton)
    {
        _dialog.DefaultButton = defaultButton;
        return this;
    }

    /// <summary>
    /// Enables the <see cref="ContentDialogButton.Close"/> button and sets the text to be displayed.
    /// </summary>
    /// <param name="text"> The text to be shown on the button </param>
    /// <param name="isEnabled"> An observable which is bound to the button and can enable/disable it </param>
    /// <param name="onClick"> A callback function on button click. Returning 'false' aborts the close operation </param>
    /// <returns> The same <see cref="ContentDialogBuilder{TContent}"/> instance </returns>
    public ContentDialogBuilder<TContent> SetCloseButton(string text,
        IObservable<bool>? isEnabled = null,
        Func<TContent, bool>? onClick = null)
    {
        _dialog.CloseButtonText = text;
        if (isEnabled is not null)
        {
            _dialog[!ContentDialog.IsPrimaryButtonEnabledProperty] = isEnabled.ToBinding();
        }

        if (onClick is not null)
        {
            _dialog.PrimaryButtonClick += (_,args) =>
            {
                var shouldClose = onClick(Content);
                args.Cancel = !shouldClose;
            };
        }
        return this;
    }

    /// <summary>
    /// Enables the <see cref="ContentDialogButton.Primary"/> button and sets the text to be displayed.
    /// </summary>
    /// <param name="text"> The text to be shown on the button </param>
    /// <param name="isEnabled"> An observable which is bound to the button and can enable/disable it </param>
    /// <param name="onClick"> A callback function on button click. Returning 'false' aborts the close operation </param>
    /// <returns> The same <see cref="ContentDialogBuilder{TContent}"/> instance </returns>
    public ContentDialogBuilder<TContent> SetPrimaryButton(string text,
        IObservable<bool>? isEnabled = null,
        Func<TContent, CancellationToken, Task<bool>>? onClick = null)
    {
        _dialog.PrimaryButtonText = text;
        BehaviorSubject<bool> notExecutingSubject = _dialogService.RegisterDisposable(new BehaviorSubject<bool>(true));
        if (isEnabled is not null)
        {
            _dialog[!ContentDialog.IsPrimaryButtonEnabledProperty] = isEnabled
                .CombineLatest(notExecutingSubject)
                .Select(x => x.First && x.Second)
                .ToBinding();
        }

        if (onClick is not null)
        {
            _dialog.PrimaryButtonClick += async (_,args) =>
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

    /// <summary>
    /// Enables the <see cref="ContentDialogButton.Secondary"/> button and sets the text to be displayed.
    /// </summary>
    /// <param name="text"> The text to be shown on the button </param>
    /// <param name="isEnabled"> An observable which is bound to the button and can enable/disable it </param>
    /// <param name="onClick"> A callback function on button click. Returning 'false' aborts the close operation </param>
    /// <returns> The same <see cref="ContentDialogBuilder{TContent}"/> instance </returns>
    public ContentDialogBuilder<TContent> SetSecondaryButton(string text,
        IObservable<bool>? isEnabled = null,
        Func<TContent, CancellationToken, Task<bool>>? onClick = null)
    {
        _dialog.SecondaryButtonText = text;
        BehaviorSubject<bool> notExecutingSubject = _dialogService.RegisterDisposable(new BehaviorSubject<bool>(true));
        if (isEnabled is not null)
        {
            _dialog[!ContentDialog.IsSecondaryButtonEnabledProperty] = isEnabled
                .CombineLatest(notExecutingSubject)
                .Select(x => x.First && x.Second)
                .ToBinding();
        }

        if (onClick is not null)
        {
            _dialog.SecondaryButtonClick += async (_,args) =>
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

    /// <summary> Show the current dialog </summary>
    /// <param name="cancellationToken"> The CancellationToken to cancel the operation </param>
    /// <returns> The task with the <see cref="ContentDialogResult"/> after the dialog has closed </returns>
    public async Task<ContentDialogResult> ShowAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _cancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            return await _dialog.ShowAsync().ConfigureAwait(true);
        }
        finally
        {
            _cancelTokenSource?.Dispose();
            _cancelTokenSource = null;
        }
    }
}
