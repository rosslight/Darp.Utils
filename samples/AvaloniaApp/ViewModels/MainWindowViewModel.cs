namespace AvaloniaApp.ViewModels;

using System.Globalization;
using Localization;

public partial class MainWindowViewModel : ViewModelBase
{
    public Resources I18N { get; } = Resources.Default;

    public string Greeting => "";//I18N.FormatAsd_Ff("f", "ff");

    public MainWindowViewModel()
    {
        I18N.CultureChanged += (_,_ ) =>
        {
            OnPropertyChanged(nameof(Greeting));
        };
    }


    public void SetLanguage(string? langCode)
    {
        if (langCode is null) return;
        I18N.Culture = CultureInfo.GetCultureInfo(langCode);
    }
}
