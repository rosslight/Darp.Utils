namespace AvaloniaApp;

using Avalonia.Controls;
using AvaloniaApp.ViewModels;
using Darp.Utils.Avalonia;
using Views;

public class ViewLocator : ViewLocatorBase<ViewModelBase>
{
    protected override Control? Build(ViewModelBase viewModel) =>
        viewModel switch
        {
            MainWindowViewModel vm => new MainWindow { ViewModel = vm },
            _ => null,
        };
}
