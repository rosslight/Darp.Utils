namespace AvaloniaApp.ViewModels;

using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Localization;

public partial class MainWindowViewModel : ViewModelBase
{
    internal Resources I18N { get; } = Resources.Default;

    [ObservableProperty] private string _greeting;

    public MainWindowViewModel()
    {
        Greeting = I18N.FormatAsd_Ff("f", "ff");
        I18N.CultureUpdated += (_,_ ) =>
        {
            Greeting = I18N.FormatAsd_Ff("f", "ff");
        };
    }


    public void SetLanguage(string? langCode)
    {
        if (langCode is null) return;
        I18N.Culture = CultureInfo.GetCultureInfo(langCode);
    }
}
