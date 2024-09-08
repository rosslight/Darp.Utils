namespace Darp.Utils.Dialog;

/// <summary> Extensions which add additional behavior to the <see cref="IDialogService"/> </summary>
public static class DialogServiceExtensions
{
    /// <summary>
    /// Create a new dialog builder based on a ContentDialog. The content is created from the given type
    /// </summary>
    /// <param name="dialogService"> The <see cref="IDialogService"/> to create the dialog from </param>
    /// <param name="title"> The title of the dialog </param>
    /// <typeparam name="TContent"> The type of the content </typeparam>
    /// <returns> The <see cref="IContentDialogBuilder{TContent}"/> </returns>
    public static IContentDialogBuilder<TContent> CreateContentDialog<TContent>(this IDialogService dialogService, string title)
        where TContent : new()
    {
        ArgumentNullException.ThrowIfNull(dialogService);
        return dialogService.CreateContentDialog(title, new TContent());
    }

    /// <summary>
    /// Create a new messagebox dialog builder based on a ContentDialog. The content is the given message
    /// </summary>
    /// <param name="dialogService"> The <see cref="IDialogService"/> to create the dialog from </param>
    /// <param name="title"> The title of the dialog </param>
    /// <param name="message"> The message to be shown </param>
    /// <param name="isSelectable"> If true, a selectable TextBlock will be used to show the message </param>
    /// <returns> The <see cref="IContentDialogBuilder{TContent}"/> </returns>
    public static IContentDialogBuilder<MessageBoxModel> CreateMessageBoxDialog(this IDialogService dialogService,
        string title,
        string message,
        bool isSelectable = false)
    {
        ArgumentNullException.ThrowIfNull(dialogService);
        return dialogService
            .CreateContentDialog(title, new MessageBoxModel { Message = message, IsSelectable = isSelectable, })
            .SetDefaultButton(ContentDialogButton.Primary)
            .SetCloseButton("Cancel")
            .SetPrimaryButton("Ok");
    }
}
