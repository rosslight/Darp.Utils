using Avalonia.Controls;

namespace EditorSample.Views;

using Avalonia.Interactivity;
using Avalonia.Styling;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        RequestedThemeVariant = RequestedThemeVariant == ThemeVariant.Light ? ThemeVariant.Dark : ThemeVariant.Light;
    }
}
