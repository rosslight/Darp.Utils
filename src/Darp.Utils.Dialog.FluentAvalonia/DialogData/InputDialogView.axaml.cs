namespace Darp.Utils.Dialog.FluentAvalonia.DialogData;

using Avalonia.Controls;
using Darp.Utils.Dialog.DialogData;

/// <summary> A view for <see cref="InputDialogViewModel"/> </summary>
public partial class InputDialogView : UserControl
{
    /// <summary> Initialize a new instance </summary>
    public InputDialogView()
    {
        InitializeComponent();
    }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        if (DataContext is not InputDialogViewModel inputData)
        {
            return;
        }

        if (inputData.IsPasswordInput)
        {
            InputBox.PasswordChar = '*';
            InputBox.Classes.Add("revealPasswordButton");
        }
        base.OnInitialized();
    }
}
