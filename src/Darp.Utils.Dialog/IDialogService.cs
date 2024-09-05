namespace Darp.Utils.Dialog;

public interface IDialogService
{
    ContentDialogBuilder<TContent> CreateContentDialog<TContent>(string title, TContent content);
}
