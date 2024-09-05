namespace Darp.Utils.Dialog;

using FluentAvalonia.UI.Controls;

public static class DialogServiceExtensions
{
    public static ContentDialogBuilder<T> CreateContentDialog<T>(this IDialogService dialogService, string title)
        where T : new()
    {
        return dialogService.CreateContentDialog(title, new T());
    }

    public static ContentDialogBuilder<MessageBoxViewModel> CreateMessageBoxDialog(this IDialogService dialogService,
        string title,
        string message,
        bool isSelectable = false)
    {
        return dialogService.CreateContentDialog(title, new MessageBoxViewModel(message, isSelectable))
            .SetDefaultButton(ContentDialogButton.Primary)
            .SetCloseButton("Cancel")
            .SetPrimaryButton("Ok");
    }
}