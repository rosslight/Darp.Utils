namespace Darp.Utils.Dialog;

using System.Diagnostics.CodeAnalysis;

/// <summary> The builder based on a content dialog </summary>
/// <typeparam name="TContent"> The type of the content </typeparam>
public interface IContentDialogBuilder<TContent>
{
    /// <summary> The content of the dialog </summary>
    string Title { get; }

    /// <summary> The content of the dialog </summary>
    TContent Content { get; }

    /// <summary> Sets the title of the dialog. The observable allows the dialog to react to changes </summary>
    /// <param name="observable"> The observable to supply the title </param>
    /// <returns> The same <see cref="IContentDialogBuilder{TContent}"/> instance </returns>
    IContentDialogBuilder<TContent> SetTitle(IObservable<string> observable);

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
    IContentDialogBuilder<TContent> SetCloseButton(string text, Func<TContent, bool>? onClick = null);

    /// <summary>
    /// Enables the <see cref="ContentDialogButton.Primary"/> button and sets the text to be displayed.
    /// </summary>
    /// <param name="text"> The text to be shown on the button </param>
    /// <param name="isEnabled"> An observable which is bound to the button and can enable/disable it </param>
    /// <param name="onClick"> A callback function on button click. Returning 'false' aborts the close operation </param>
    /// <returns> The same <see cref="IContentDialogBuilder{TContent}"/> instance </returns>
    IContentDialogBuilder<TContent> SetPrimaryButton(
        string text,
        IObservable<bool>? isEnabled = null,
        Func<TContent, CancellationToken, Task<bool>>? onClick = null
    );

    /// <summary>
    /// Enables the <see cref="ContentDialogButton.Secondary"/> button and sets the text to be displayed.
    /// </summary>
    /// <param name="text"> The text to be shown on the button </param>
    /// <param name="isEnabled"> An observable which is bound to the button and can enable/disable it </param>
    /// <param name="onClick"> A callback function on button click. Returning 'false' aborts the close operation </param>
    /// <returns> The same <see cref="IContentDialogBuilder{TContent}"/> instance </returns>
    IContentDialogBuilder<TContent> SetSecondaryButton(
        string text,
        IObservable<bool>? isEnabled = null,
        Func<TContent, CancellationToken, Task<bool>>? onClick = null
    );

    /// <summary> Enables / disables closing when pressing the escape key </summary>
    /// <param name="isClosing"> True, if closing is allowed. False otherwise </param>
    /// <returns> The same <see cref="IContentDialogBuilder{TContent}"/> instance </returns>
    IContentDialogBuilder<TContent> SetClosingOnEscape(bool isClosing);

    /// <summary> Show the current dialog </summary>
    /// <param name="cancellationToken"> The CancellationToken to cancel showing the dialog </param>
    /// <returns> The task with the <see cref="ContentDialogResult{TContent}"/> after the dialog has closed </returns>
    DialogAwaitable<TContent> ShowAsync(CancellationToken cancellationToken = default);
}

/// <summary> A dialog result which contains the content </summary>
/// <param name="Result"> The result status </param>
/// <param name="Content"> The content </param>
/// <typeparam name="TContent"> The type of the <see cref="Content"/> </typeparam>
[method: SetsRequiredMembers]
public readonly record struct ContentDialogResult<TContent>(ContentDialogResult Result, TContent Content)
{
    /// <summary> The result status </summary>
    public required ContentDialogResult Result { get; init; } = Result;

    /// <summary> The content </summary>
    public required TContent Content { get; init; } = Content;

    /// <summary> The dialog closed with result <see cref="ContentDialogResult.None"/> </summary>
    public bool IsNone => Result is ContentDialogResult.None;

    /// <summary> The dialog closed with result <see cref="ContentDialogResult.Primary"/> </summary>
    public bool IsPrimary => Result is ContentDialogResult.Primary;

    /// <summary> The dialog closed with result <see cref="ContentDialogResult.Secondary"/> </summary>
    public bool IsSecondary => Result is ContentDialogResult.Secondary;
}
