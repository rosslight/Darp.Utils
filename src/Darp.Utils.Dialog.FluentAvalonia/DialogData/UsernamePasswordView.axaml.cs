namespace Darp.Utils.Dialog.FluentAvalonia.DialogData;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Darp.Utils.Dialog.DialogData;

/// <summary> A view for <see cref="InputDialogViewModel"/> </summary>
public partial class UsernamePasswordView : UserControl
{
    /// <summary> Initialize a new instance </summary>
    public UsernamePasswordView()
    {
        InitializeComponent();
    }

    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e)
    {
        Dispatcher.UIThread.Post(() => UsernameTextBox.Focus());
        PasswordTextBox.TextChanged += (sender, args) =>
        {
            if (DataContext is UsernamePasswordViewModel vm)
                vm.ClearPasswordValidationError();
        };
        base.OnLoaded(e);
    }

    /// <inheritdoc />
    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is UsernamePasswordViewModel vm)
        {
            vm.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName is nameof(UsernamePasswordViewModel.Step))
                {
                    if (vm.Step is UsernamePasswordStep.RequestUsername)
                        Dispatcher.UIThread.Post(() => UsernameTextBox.Focus());
                    else if (vm.Step is UsernamePasswordStep.RequestPassword)
                        Dispatcher.UIThread.Post(() => PasswordTextBox.Focus());
                }
            };
        }
        base.OnDataContextChanged(e);
    }
}
