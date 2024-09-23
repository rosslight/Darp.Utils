namespace AvaloniaApp.Localization;

using System.ComponentModel;

public partial class Resources : INotifyPropertyChanged
{
    public Resources()
    {
        CultureChanged += (_, _) =>
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        };
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
