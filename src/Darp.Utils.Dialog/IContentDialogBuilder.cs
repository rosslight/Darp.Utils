namespace Darp.Utils.Dialog;

/// <summary> The builder based on a content dialog </summary>
/// <typeparam name="TContent"> The type of the content </typeparam>
public interface IContentDialogBuilder<out TContent>
{
    /// <summary> The content of the dialog </summary>
    string Title { get; }
    /// <summary> The content of the dialog </summary>
    TContent Content { get; }

    /// <summary> Sets the button which is shown as the default button </summary>
    /// <param name="defaultButton"> The default <see cref="ContentDialogButton"/> </param>
    /// <returns> The same <see cref="IContentDialogBuilder{TContent}"/> instance </returns>
    IContentDialogBuilder<TContent> SetDefaultButton(ContentDialogButton defaultButton);

    /// <summary>
    /// Enables the <see cref="ContentDialogButton.Close"/> button and sets the text to be displayed.
    /// </summary>
    /// <param name="text"> The text to be shown on the button </param>
    /// <param name="onClick"> A callback function on button click. Returning 'false' aborts the close operation </param>
    /// <returns> The same <see cref="IContentDialogBuilder{TContent}"/> instance </returns>
    IContentDialogBuilder<TContent> SetCloseButton(string text,
        Func<TContent, bool>? onClick = null);

    /// <summary>
    /// Enables the <see cref="ContentDialogButton.Primary"/> button and sets the text to be displayed.
    /// </summary>
    /// <param name="text"> The text to be shown on the button </param>
    /// <param name="isEnabled"> An observable which is bound to the button and can enable/disable it </param>
    /// <param name="onClick"> A callback function on button click. Returning 'false' aborts the close operation </param>
    /// <returns> The same <see cref="IContentDialogBuilder{TContent}"/> instance </returns>
    IContentDialogBuilder<TContent> SetPrimaryButton(string text,
        IObservable<bool>? isEnabled = null,
        Func<TContent, CancellationToken, Task<bool>>? onClick = null);

    /// <summary>
    /// Enables the <see cref="ContentDialogButton.Secondary"/> button and sets the text to be displayed.
    /// </summary>
    /// <param name="text"> The text to be shown on the button </param>
    /// <param name="isEnabled"> An observable which is bound to the button and can enable/disable it </param>
    /// <param name="onClick"> A callback function on button click. Returning 'false' aborts the close operation </param>
    /// <returns> The same <see cref="IContentDialogBuilder{TContent}"/> instance </returns>
    IContentDialogBuilder<TContent> SetSecondaryButton(string text,
        IObservable<bool>? isEnabled = null,
        Func<TContent, CancellationToken, Task<bool>>? onClick = null);

    /// <summary> Show the current dialog </summary>
    /// <param name="cancellationToken"> The CancellationToken to cancel the operation </param>
    /// <returns> The task with the <see cref="ContentDialogResult"/> after the dialog has closed </returns>
    Task<ContentDialogResult> ShowAsync(CancellationToken cancellationToken = default);
}
