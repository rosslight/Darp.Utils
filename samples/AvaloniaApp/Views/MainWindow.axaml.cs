namespace AvaloniaApp.Views;

using Avalonia.Interactivity;
using Avalonia.Styling;
using AvaloniaApp.ViewModels;
using Darp.Utils.Avalonia;

public partial class MainWindow : WindowBase<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void ToggleTheme_OnClick(object? sender, RoutedEventArgs e)
    {
        RequestedThemeVariant = RequestedThemeVariant == ThemeVariant.Light ? ThemeVariant.Dark : ThemeVariant.Light;
    }
}
