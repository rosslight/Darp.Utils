namespace EditorSample.ViewModels;

using CommunityToolkit.Mvvm.Input;

public partial class MainWindowViewModel : ViewModelBase
{
    public string Greeting { get; } = "Welcome to Avalonia!";

    public CodeMirrorService CodeMirror { get; } = new();

    [RelayCommand]
    private void X(string a)
    {
        int i = 0;
    }
}
