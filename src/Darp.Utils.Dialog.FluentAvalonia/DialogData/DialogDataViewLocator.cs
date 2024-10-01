namespace Darp.Utils.Dialog.FluentAvalonia.DialogData;

using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Darp.Utils.Dialog.DialogData;

/// <summary>
/// A simple viewLocator used to match DialogData views
/// </summary>
public sealed class DialogDataViewLocator : IDataTemplate
{
    /// <inheritdoc />
    public Control Build(object? param) =>
        param switch
        {
            not IDialogData => new TextBlock
            {
                Text = "No DialogData provided. This should not happen (because of Match)",
            },
            InputDialogData vm => new InputDialogView { DataContext = vm },
            MessageBoxModel vm => new MessageBoxView { DataContext = vm },
            _ => new TextBlock { Text = $"DialogData is not registered: {param.GetType()}" },
        };

    /// <inheritdoc />
    public bool Match(object? data) => data is IDialogData;
}
