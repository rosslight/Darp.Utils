namespace Darp.Utils.Avalonia;

using System.Diagnostics.CodeAnalysis;
using global::Avalonia;
using global::Avalonia.Controls;

/// <summary> A base class which provides a typed <see cref="ViewModel"/> property for an <see cref="UserControl"/> </summary>
/// <typeparam name="TViewModel"> The type of the ViewModel </typeparam>
public abstract class UserControlBase<TViewModel> : UserControl
    where TViewModel : IViewModelBase
{
    /// <inheritdoc cref="StyledElement.DataContext" />
    public new object? DataContext
    {
        get => base.DataContext;
        private set => base.DataContext = value;
    }

    /// <summary> The viewModel of the control. Setting the viewModel sets the DataContext as well. </summary>
    /// <exception cref="InvalidOperationException"> Thrown if the DataContext is not set </exception>
    public virtual TViewModel ViewModel
    {
        get => AvaloniaHelpers.GetViewModel<TViewModel>(base.DataContext);
        [MemberNotNull(nameof(base.DataContext))]
        init => DataContext = value;
    }
}
