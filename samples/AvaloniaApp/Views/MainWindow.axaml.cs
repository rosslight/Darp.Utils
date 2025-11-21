namespace AvaloniaApp.Views;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using AvaloniaApp.ViewModels;
using Darp.Utils.Avalonia;
using FluentAvalonia.UI.Controls;

public sealed partial class MainWindow : WindowBase<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
        MyFrame.NavigationPageFactory = new DependencyInjectionPageFactory(this);
    }

    private void ToggleTheme_OnClick(object? sender, RoutedEventArgs e)
    {
        RequestedThemeVariant = RequestedThemeVariant == ThemeVariant.Light ? ThemeVariant.Dark : ThemeVariant.Light;
    }

    private void NavigationView_OnSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
    {
        MyFrame.Navigate((e.SelectedItem as NavigationViewItem)?.Tag?.GetType() ?? typeof(string));
    }
}

file sealed class DependencyInjectionPageFactory(MainWindow window) : INavigationPageFactory
{
    private readonly MainWindow _window = window;

    public Control GetPage(Type srcType)
    {
        if (srcType != typeof(string))
            return new UserControl { Content = "Heyaaa" };
        return new UserControl
        {
            [!ContentControl.ContentProperty] = _window.GetResourceObservable("MyContent").ToBinding(),
        };
    }

    public Control GetPageFromObject(object target) => throw new NotSupportedException();
}
