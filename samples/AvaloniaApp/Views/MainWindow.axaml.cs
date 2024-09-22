namespace AvaloniaApp.Views;

using Avalonia.Controls;
using AvaloniaApp.ViewModels;

public partial class MainWindow : Window
{
    public required MainWindowViewModel ViewModel { get; init; }

    public MainWindow()
    {
        InitializeComponent();
    }
}
