using Avalonia.Controls;

namespace EditorSample.Views;

using Avalonia.Interactivity;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        EditorView.Address = $"http://localhost:{BackendInfo.Port}/index.html";
        EditorView.ExecuteScriptFunction("setTheme", "dark");
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        EditorView.ExecuteScript("""window.setTheme("dark");""");
    }
}
