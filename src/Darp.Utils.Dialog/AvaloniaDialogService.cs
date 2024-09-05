namespace Darp.Utils.Dialog;

public sealed class AvaloniaDialogService : IDialogService
{
    public ContentDialogBuilder<TContent> CreateContentDialog<TContent>(string title, TContent content)
    {
        return new ContentDialogBuilder<TContent>(title, content);
    }
}