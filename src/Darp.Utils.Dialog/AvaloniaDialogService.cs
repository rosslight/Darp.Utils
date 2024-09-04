namespace Darp.Utils.Dialog;

using FluentAvalonia.UI.Controls;

public sealed class AvaloniaDialogService : IDialogService
{
    public ContentDialogBuilder<TContent, DialogUnit, DialogUnit> CreateContentDialog<TContent>(string title, TContent content)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
        };
        return new ContentDialogBuilder<TContent, DialogUnit, DialogUnit>(this, dialog, content);
    }
}