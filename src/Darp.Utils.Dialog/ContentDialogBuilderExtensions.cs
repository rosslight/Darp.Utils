namespace Darp.Utils.Dialog;

using System.ComponentModel;

public static class ContentDialogBuilderExtensions
{
    public static ContentDialogBuilder<TContent, DialogUnit, TSecondary> SetPrimaryButton<TContent, TSecondary>(
        this ContentDialogBuilder<TContent, DialogUnit, TSecondary> builder,
        string text,
        IObservable<bool>? isEnabled = null,
        Func<TContent, bool>? shouldClose = null)
        where TContent : INotifyPropertyChanged
        where TSecondary : notnull
    {
        return builder.SetPrimaryButton<DialogUnit>(text, isEnabled, shouldClose is null
            ? null
            : (content, _) => Task.FromResult(DialogCloseResult<DialogUnit>.CreateCloseRequested(shouldClose(content))));
    }

    public static ContentDialogBuilder<TContent, DialogUnit, TSecondary> SetPrimaryButton<TContent, TSecondary>(
        this ContentDialogBuilder<TContent, DialogUnit, TSecondary> builder,
        string text,
        IObservable<bool>? isEnabled = null,
        Func<TContent, CancellationToken, Task<bool>>? shouldClose = null)
        where TContent : INotifyPropertyChanged
        where TSecondary : notnull
    {
        return builder.SetPrimaryButton<DialogUnit>(text, isEnabled, shouldClose is null
            ? null
            : async (content, token) => DialogCloseResult<DialogUnit>.CreateCloseRequested(await shouldClose(content, token)));
    }
}