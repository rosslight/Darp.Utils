namespace Darp.Utils.Dialog;

using System.Diagnostics.CodeAnalysis;
using DialogData;
using Darp.Utils.Dialog.Helper;

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
            .SetPrimaryButton("Ok");
    }

    /// <summary>
    /// Create a new input dialog builder based on a ContentDialog. The content is an optional message and the input
    /// </summary>
    /// <param name="dialogService"> The <see cref="IDialogService"/> to create the dialog from </param>
    /// <param name="title"> The title of the dialog </param>
    /// <param name="message"> The optional message to be shown on top of the input </param>
    /// <param name="isMessageSelectable"> If true, a selectable TextBlock will be used to show the message </param>
    /// <returns> The <see cref="IContentDialogBuilder{TContent}"/> </returns>
    [RequiresUnreferencedCode("This method requires the generated CommunityToolkit. Mvvm. ComponentModel.__Internals.__ObservableValidatorExtensions type not to be removed to use the fast path")]
    public static IContentDialogBuilder<InputDialogData> CreateInputDialog(this IDialogService dialogService,
        string title,
        string? message = null,
        bool isMessageSelectable = false)
    {
        ArgumentNullException.ThrowIfNull(dialogService);
        var dialogData = new InputDialogData { Message = message, IsMessageSelectable = isMessageSelectable };
        return dialogService
            .CreateContentDialog(title, dialogData)
            .SetDefaultButton(ContentDialogButton.Primary)
            .SetCloseButton("Cancel")
            .SetPrimaryButton("Ok", dialogData.WhenPropertyChanged(x => x.HasErrors, x => !x));
    }
}
