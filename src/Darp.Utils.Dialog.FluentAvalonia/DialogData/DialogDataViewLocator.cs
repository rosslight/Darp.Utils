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
            InputDialogViewModel vm => new InputDialogView { DataContext = vm },
            MessageBoxViewModel vm => new MessageBoxView { DataContext = vm },
            UsernamePasswordViewModel vm => new UsernamePasswordView { DataContext = vm },
            _ => new TextBlock { Text = $"DialogData is not registered: {param?.GetType()}" },
        };

    /// <inheritdoc />
    public bool Match(object? data) => data is InputDialogViewModel or MessageBoxViewModel or UsernamePasswordViewModel;
}
