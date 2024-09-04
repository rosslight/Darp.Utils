namespace Darp.Utils.Dialog;

public static class DialogServiceExtensions
{
    public static ContentDialogBuilder<T, DialogUnit, DialogUnit> CreateContentDialog<T>(this IDialogService dialogService, string title)
        where T : new()
    {
        return dialogService.CreateContentDialog(title, new T());
    }
}