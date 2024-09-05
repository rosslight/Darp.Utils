namespace Darp.Utils.Dialog;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;

public sealed class ContentDialogBuilder<TContent>
{
    private readonly ContentDialog _dialog;
    public TContent Content { get; }

    private CancellationTokenSource? _cancelTokenSource;

    public ContentDialogBuilder(string title, TContent content)
    {
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
            if (firstOrDefault is null) return;
            firstOrDefault.Focus();
        };
        _dialog.DataTemplates.Add(new FuncDataTemplate<MessageBoxViewModel>((model, _) => model.IsSelectable
            ? new SelectableTextBlock { Text = model.Message, TextWrapping = TextWrapping.Wrap }
            : new TextBlock { Text = model.Message, TextWrapping = TextWrapping.Wrap }));
        Content = content;
    }

    public ContentDialogBuilder<TContent> SetDefaultButton(ContentDialogButton defaultButton)
    {
        _dialog.DefaultButton = defaultButton;
        return this;
    }

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
                bool shouldClose = onClick(Content);
                args.Cancel = !shouldClose;
            };
        }
        return this;
    }

    public ContentDialogBuilder<TContent> SetPrimaryButton(string text,
        IObservable<bool>? isEnabled = null,
        Func<TContent, CancellationToken, Task<bool>>? onClick = null)
    {
        _dialog.PrimaryButtonText = text;
        if (isEnabled is not null)
        {
            _dialog[!ContentDialog.IsPrimaryButtonEnabledProperty] = isEnabled.ToBinding();
        }

        if (onClick is not null)
        {
            _dialog.PrimaryButtonClick += async (_,args) =>
            {
                Deferral? deferral = null;
                try
                {
                    deferral = args.GetDeferral();
                    bool shouldClose = await onClick(Content, _cancelTokenSource?.Token ?? default);
                    args.Cancel = !shouldClose;
                }
                finally
                {
                    deferral?.Complete();
                }
            };
        }
        return this;
    }

    public ContentDialogBuilder<TContent> SetSecondaryButton(string text,
        IObservable<bool>? isEnabled = null,
        Func<TContent, CancellationToken, Task<bool>>? onClick = null)
    {
        _dialog.SecondaryButtonText = text;
        if (isEnabled is not null)
        {
            _dialog[!ContentDialog.IsSecondaryButtonEnabledProperty] = isEnabled.ToBinding();
        }

        if (onClick is not null)
        {
            _dialog.SecondaryButtonClick += async (_,args) =>
            {
                Deferral? deferral = null;
                try
                {
                    deferral = args.GetDeferral();
                    bool shouldClose = await onClick(Content, _cancelTokenSource?.Token ?? default);
                    args.Cancel = !shouldClose;
                }
                finally
                {
                    deferral?.Complete();
                }
            };
        }
        return this;
    }

    public async Task<ContentDialogResult> ShowAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _cancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            return await _dialog.ShowAsync();
        }
        finally
        {
            _cancelTokenSource?.Dispose();
            _cancelTokenSource = null;
        }
    }
}