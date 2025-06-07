namespace EditorSample.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string Text { get; set; } = "// Enter your C# here!";

    public CodeMirrorService CodeMirror { get; } = new();

    [RelayCommand]
    private void X(string a)
    {
        int i = 0;
    }
}
