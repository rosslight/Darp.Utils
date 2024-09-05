namespace Darp.Utils.Dialog;

using System.ComponentModel;

public static class ContentDialogBuilderExtensions
{
    public static ContentDialogBuilder<TContent> SetPrimaryButton<TContent>(
        this ContentDialogBuilder<TContent> builder,
        string text,
        IObservable<bool>? isEnabled = null,
        Func<TContent, bool>? onClick = null)
        where TContent : INotifyPropertyChanged
    {
        return builder.SetPrimaryButton(text, isEnabled, onClick is null
            ? null
            : (content, _) => Task.FromResult(onClick(content)));
    }

    public static ContentDialogBuilder<TContent> SetPrimaryButton<TContent>(
        this ContentDialogBuilder<TContent> builder,
        string text,
        IObservable<bool>? isEnabled = null,
        Func<TContent, Task<bool>>? shouldClose = null)
        where TContent : INotifyPropertyChanged
    {
        return builder.SetPrimaryButton(text, isEnabled, shouldClose is null
            ? null
            : (content, _) => shouldClose(content));
    }

    public static ContentDialogBuilder<TContent> SetSecondaryButton<TContent>(
        this ContentDialogBuilder<TContent> builder,
        string text,
        IObservable<bool>? isEnabled = null,
        Func<TContent, bool>? onClick = null)
        where TContent : INotifyPropertyChanged
    {
        return builder.SetSecondaryButton(text, isEnabled, onClick is null
            ? null
            : (content, _) => Task.FromResult(onClick(content)));
    }

    public static ContentDialogBuilder<TContent> SetSecondaryButton<TContent>(
        this ContentDialogBuilder<TContent> builder,
        string text,
        IObservable<bool>? isEnabled = null,
        Func<TContent, Task<bool>>? shouldClose = null)
        where TContent : INotifyPropertyChanged
    {
        return builder.SetSecondaryButton(text, isEnabled, shouldClose is null
            ? null
            : (content, _) => shouldClose(content));
    }
}