namespace Darp.Utils.Dialog;

using System.ComponentModel;
using FluentAvalonia.UI.Controls;

/// <summary> Extensions of the <see cref="ContentDialogBuilder{TContent}"/> </summary>
public static class ContentDialogBuilderExtensions
{
    /// <summary>
    /// Enables the <see cref="ContentDialogButton.Primary"/> button and sets the text to be displayed.
    /// Provides a synchronous onClick function
    /// </summary>
    /// <param name="builder"> The parent <see cref="ContentDialogBuilder{TContent}"/> </param>
    /// <param name="text"> The text to be shown on the button </param>
    /// <param name="isEnabled"> An observable which is bound to the button and can enable/disable it </param>
    /// <param name="onClick"> A callback function on button click. Returning 'false' aborts the close operation </param>
    /// <returns> The same <see cref="ContentDialogBuilder{TContent}"/> instance </returns>
    /// <typeparam name="TContent"> The type of the content </typeparam>
    public static ContentDialogBuilder<TContent> SetPrimaryButton<TContent>(
        this ContentDialogBuilder<TContent> builder,
        string text,
        IObservable<bool>? isEnabled = null,
        Func<TContent, bool>? onClick = null)
        where TContent : INotifyPropertyChanged
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.SetPrimaryButton(text, isEnabled, onClick is null
            ? null
            : (content, _) => Task.FromResult(onClick(content)));
    }

    /// <summary>
    /// Enables the <see cref="ContentDialogButton.Primary"/> button and sets the text to be displayed.
    /// Provides an asynchronous onClick function without <see cref="CancellationToken"/>
    /// </summary>
    /// <param name="builder"> The parent <see cref="ContentDialogBuilder{TContent}"/> </param>
    /// <param name="text"> The text to be shown on the button </param>
    /// <param name="isEnabled"> An observable which is bound to the button and can enable/disable it </param>
    /// <param name="onClick"> A callback function on button click. Returning 'false' aborts the close operation </param>
    /// <returns> The same <see cref="ContentDialogBuilder{TContent}"/> instance </returns>
    /// <typeparam name="TContent"> The type of the content </typeparam>
    public static ContentDialogBuilder<TContent> SetPrimaryButton<TContent>(
        this ContentDialogBuilder<TContent> builder,
        string text,
        IObservable<bool>? isEnabled = null,
        Func<TContent, Task<bool>>? onClick = null)
        where TContent : INotifyPropertyChanged
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.SetPrimaryButton(text, isEnabled, onClick is null
            ? null
            : (content, _) => onClick(content));
    }

    /// <summary>
    /// Enables the <see cref="ContentDialogButton.Secondary"/> button and sets the text to be displayed.
    /// Provides a synchronous onClick function
    /// </summary>
    /// <param name="builder"> The parent <see cref="ContentDialogBuilder{TContent}"/> </param>
    /// <param name="text"> The text to be shown on the button </param>
    /// <param name="isEnabled"> An observable which is bound to the button and can enable/disable it </param>
    /// <param name="onClick"> A callback function on button click. Returning 'false' aborts the close operation </param>
    /// <returns> The same <see cref="ContentDialogBuilder{TContent}"/> instance </returns>
    /// <typeparam name="TContent"> The type of the content </typeparam>
    public static ContentDialogBuilder<TContent> SetSecondaryButton<TContent>(
        this ContentDialogBuilder<TContent> builder,
        string text,
        IObservable<bool>? isEnabled = null,
        Func<TContent, bool>? onClick = null)
        where TContent : INotifyPropertyChanged
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.SetSecondaryButton(text, isEnabled, onClick is null
            ? null
            : (content, _) => Task.FromResult(onClick(content)));
    }

    /// <summary>
    /// Enables the <see cref="ContentDialogButton.Secondary"/> button and sets the text to be displayed.
    /// Provides an asynchronous onClick function without <see cref="CancellationToken"/>
    /// </summary>
    /// <param name="builder"> The parent <see cref="ContentDialogBuilder{TContent}"/> </param>
    /// <param name="text"> The text to be shown on the button </param>
    /// <param name="isEnabled"> An observable which is bound to the button and can enable/disable it </param>
    /// <param name="onClick"> A callback function on button click. Returning 'false' aborts the close operation </param>
    /// <returns> The same <see cref="ContentDialogBuilder{TContent}"/> instance </returns>
    /// <typeparam name="TContent"> The type of the content </typeparam>
    public static ContentDialogBuilder<TContent> SetSecondaryButton<TContent>(
        this ContentDialogBuilder<TContent> builder,
        string text,
        IObservable<bool>? isEnabled = null,
        Func<TContent, Task<bool>>? onClick = null)
        where TContent : INotifyPropertyChanged
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.SetSecondaryButton(text, isEnabled, onClick is null
            ? null
            : (content, _) => onClick(content));
    }
}
