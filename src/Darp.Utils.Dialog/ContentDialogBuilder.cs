namespace Darp.Utils.Dialog;

using Avalonia;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;

public sealed class ContentDialogBuilder<TContent, TPrimary, TSecondary>(
    AvaloniaDialogService dialogService,
    ContentDialog dialog,
    TContent content)
    where TPrimary : notnull
    where TSecondary : notnull
{
    private readonly AvaloniaDialogService _dialogService = dialogService;
    private readonly ContentDialog _dialog = dialog;
    public TContent Content { get; } = content;

    private CancellationTokenSource? _cancelTokenSource;
    private TPrimary? _currentPrimaryResult;
    private TSecondary? _currentSecondaryResult;

    public ContentDialogBuilder<TContent, TPrimary, TSecondary> SetCloseButton(string text)
    {
        _dialog.CloseButtonText = text;
        return this;
    }

    public ContentDialogBuilder<TContent, TNewPrimary, TSecondary> SetPrimaryButton<TNewPrimary>(string text,
        IObservable<bool>? isEnabled = null,
        Func<TContent, CancellationToken, Task<DialogCloseResult<TNewPrimary>>>? onClick = null)
        where TNewPrimary : notnull
    {
        var newContentDialogBuilder = new ContentDialogBuilder<TContent, TNewPrimary, TSecondary>(_dialogService, _dialog, Content);
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
                    DialogCloseResult<TNewPrimary> result = await onClick(Content, _cancelTokenSource?.Token ?? default);
                    args.Cancel = !result.IsCloseRequested;
                    newContentDialogBuilder._currentPrimaryResult = result.Result;
                }
                finally
                {
                    deferral?.Complete();
                }
            };
        }
        return newContentDialogBuilder;
    }

    public async Task<ContentDialogResult<TPrimary, TSecondary>> ShowAsync(CancellationToken cancellationToken)
    {
        try
        {
            _cancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            ContentDialogResult result = await _dialog.ShowAsync();
            return new ContentDialogResult<TPrimary, TSecondary>(result, _currentPrimaryResult, _currentSecondaryResult);
        }
        finally
        {
            _cancelTokenSource?.Dispose();
            _cancelTokenSource = null;
            _currentPrimaryResult = default;
            _currentSecondaryResult = default;
        }
    }
}