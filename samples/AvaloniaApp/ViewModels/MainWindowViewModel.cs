namespace AvaloniaApp.ViewModels;

using System.Globalization;
using CommunityToolkit.Mvvm.Input;
using Darp.Utils.Dialog;
using Darp.Utils.Dialog.DialogData;
using Localization;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;
    public Resources I18N { get; } = Resources.Default;

    public string Greeting => I18N.FormatAsd_Ff("f", "ff");

    public MainWindowViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService.WithDialogRoot(this);
        I18N.CultureChanged += (_, _) => OnPropertyChanged(nameof(Greeting));
    }

    [RelayCommand]
    private async Task SetLanguageAsync(string? langCode, CancellationToken cancellationToken)
    {
        if (langCode is null)
            return;
        ContentDialogResult<MessageBoxModel> result = await _dialogService
            .CreateMessageBoxDialog("Set language", $"Do you want to set the language to '{langCode}'?")
            .SetCloseButton("Cancel")
            .ShowAsync(cancellationToken);
        if (!result.IsPrimary)
        {
            return;
        }
        I18N.Culture = CultureInfo.GetCultureInfo(langCode);
    }
}
