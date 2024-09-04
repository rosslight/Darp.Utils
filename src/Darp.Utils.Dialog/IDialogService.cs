namespace Darp.Utils.Dialog;

public interface IDialogService
{
    ContentDialogBuilder<TContent, DialogUnit, DialogUnit> CreateContentDialog<TContent>(string title, TContent content);
}
