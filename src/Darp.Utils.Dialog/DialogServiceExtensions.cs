namespace Darp.Utils.Dialog;

using Darp.Utils.Dialog.Helper;
using DialogData;

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
    public static IContentDialogBuilder<TContent> CreateContentDialog<TContent>(
        this IDialogService dialogService,
        string title
    )
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
    public static IContentDialogBuilder<MessageBoxViewModel> CreateMessageBoxDialog(
        this IDialogService dialogService,
        string title,
        string message,
        bool isSelectable = false
    )
    {
        ArgumentNullException.ThrowIfNull(dialogService);
        return dialogService
            .CreateContentDialog(title, new MessageBoxViewModel { Message = message, IsSelectable = isSelectable })
            .SetDefaultButton(ContentDialogButton.Primary)
            .SetPrimaryButton("Ok");
    }

    /// <summary>
    /// Show a new messagebox dialog builder based on a ContentDialog. The content is the given message
    /// </summary>
    /// <param name="dialogService"> The <see cref="IDialogService"/> to create the dialog from </param>
    /// <param name="title"> The title of the dialog </param>
    /// <param name="message"> The message to be shown </param>
    /// <param name="isSelectable"> If true, a selectable TextBlock will be used to show the message </param>
    /// <param name="cancellationToken"> The cancellation token to cancel the operation </param>
    /// <returns> The <see cref="IContentDialogBuilder{TContent}"/> </returns>
    public static Task<ContentDialogResult<MessageBoxViewModel>> ShowMessageBoxDialogAsync(
        this IDialogService dialogService,
        string title,
        string message,
        bool isSelectable = true,
        CancellationToken cancellationToken = default
    ) => dialogService.CreateMessageBoxDialog(title, message, isSelectable).ShowAsync(cancellationToken);

    /// <summary>
    /// Create a new input dialog builder based on a ContentDialog. The content is an optional message and the input
    /// </summary>
    /// <param name="dialogService"> The <see cref="IDialogService"/> to create the dialog from </param>
    /// <param name="title"> The title of the dialog </param>
    /// <returns> The <see cref="IContentDialogBuilder{TContent}"/> </returns>
    public static IContentDialogBuilder<UsernamePasswordViewModel> CreateUsernamePasswordDialog(
        this IDialogService dialogService,
        string title
    )
    {
        ArgumentNullException.ThrowIfNull(dialogService);
        var dialogData = new UsernamePasswordViewModel();
        IContentDialogBuilder<UsernamePasswordViewModel> dialogBuilder = dialogService
            .CreateContentDialog(title, dialogData)
            .SetDefaultButton(ContentDialogButton.Primary)
            .SetCloseButton("Cancel")
            .SetPrimaryButton(
                "Confirm",
                isEnabled: dialogData.WhenPropertyChanged(x => x.IsCurrentStepValid),
                onClick: (model, token) => model.RequestNextStepAsync(token)
            )
            .ConfigureUsernameStep("Enter username")
            .ConfigurePasswordStep("Enter password");

        return dialogBuilder;
    }

    /// <summary>
    /// Create a new input dialog builder based on a ContentDialog. The content is an optional message and the input
    /// </summary>
    /// <param name="dialogService"> The <see cref="IDialogService"/> to create the dialog from </param>
    /// <param name="title"> The title of the dialog </param>
    /// <param name="cancellationToken"> The cancellation token to cancel the operation </param>
    /// <returns> The <see cref="IContentDialogBuilder{TContent}"/> </returns>
    public static Task<ContentDialogResult<UsernamePasswordViewModel>> ShowUsernamePasswordDialogAsync(
        this IDialogService dialogService,
        string title,
        CancellationToken cancellationToken = default
    ) => dialogService.CreateUsernamePasswordDialog(title).ShowAsync(cancellationToken);

    /// <summary>
    /// Create a new input dialog builder based on a ContentDialog. The content is an optional message and the input
    /// </summary>
    /// <param name="dialogService"> The <see cref="IDialogService"/> to create the dialog from </param>
    /// <param name="title"> The title of the dialog </param>
    /// <param name="message"> The optional message to be shown on top of the input </param>
    /// <param name="isMessageSelectable"> If true, a selectable TextBlock will be used to show the message </param>
    /// <returns> The <see cref="IContentDialogBuilder{TContent}"/> </returns>
    public static IContentDialogBuilder<InputDialogViewModel> CreateInputDialog(
        this IDialogService dialogService,
        string title,
        string? message = null,
        bool isMessageSelectable = false
    )
    {
        ArgumentNullException.ThrowIfNull(dialogService);
        var dialogData = new InputDialogViewModel { Message = message, IsMessageSelectable = isMessageSelectable };
        return dialogService
            .CreateContentDialog(title, dialogData)
            .SetDefaultButton(ContentDialogButton.Primary)
            .SetCloseButton("Cancel")
            .SetPrimaryButton("Ok", dialogData.WhenPropertyChanged(x => x.HasErrors, x => !x));
    }

    /// <summary>
    /// Show a new input dialog builder based on a ContentDialog. The content is an optional message and the input
    /// </summary>
    /// <param name="dialogService"> The <see cref="IDialogService"/> to create the dialog from </param>
    /// <param name="title"> The title of the dialog </param>
    /// <param name="message"> The optional message to be shown on top of the input </param>
    /// <param name="isMessageSelectable"> If true, a selectable TextBlock will be used to show the message </param>
    /// <param name="cancellationToken"> The cancellation token to cancel the operation </param>
    /// <returns> The <see cref="IContentDialogBuilder{TContent}"/> </returns>
    public static Task<ContentDialogResult<InputDialogViewModel>> ShowInputDialogAsync(
        this IDialogService dialogService,
        string title,
        string? message = null,
        bool isMessageSelectable = false,
        CancellationToken cancellationToken = default
    ) => dialogService.CreateInputDialog(title, message, isMessageSelectable).ShowAsync(cancellationToken);

    /// <summary>
    /// Create a new <see cref="IDialogService"/> with the dialog root set to <typeparamref name="TDialogRoot"/>
    /// </summary>
    /// <param name="dialogService"> The <see cref="IDialogService"/> to set the dialog root to </param>
    /// <typeparam name="TDialogRoot"> The type of the dialog root to be used as a key </typeparam>
    /// <returns> A new dialogService </returns>
    public static IDialogService WithDialogRoot<TDialogRoot>(this IDialogService dialogService)
    {
        ArgumentNullException.ThrowIfNull(dialogService);
        return dialogService.WithDialogRoot(typeof(TDialogRoot));
    }
}
