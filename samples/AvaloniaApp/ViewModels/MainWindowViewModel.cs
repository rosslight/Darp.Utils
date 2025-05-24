namespace AvaloniaApp.ViewModels;

using System.ComponentModel.DataAnnotations;
using System.Globalization;
using CommunityToolkit.Mvvm.Input;
using Darp.Utils.Dialog;
using Darp.Utils.Dialog.DialogData;
using Localization;

public sealed partial class MainWindowViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;
    public Resources I18N { get; } = Resources.Default;

    public string Greeting => I18N.FormatAsd_Ff("f", "ff");

    public MainWindowViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService.WithDialogRoot<MainWindowViewModel>();
        I18N.CultureChanged += (_, _) => OnPropertyChanged(nameof(Greeting));
    }

    [RelayCommand]
    private async Task SetLanguageAsync(string? langCode, CancellationToken cancellationToken)
    {
        if (langCode is null)
            return;
        ContentDialogResult<MessageBoxViewModel> result = await _dialogService
            .CreateMessageBoxDialog("Set language", $"Do you want to set the language to '{langCode}'?")
            .SetCloseButton("Cancel")
            .ShowAsync(cancellationToken);
        if (!result.IsPrimary)
            return;
        I18N.Culture = CultureInfo.GetCultureInfo(langCode);
    }

    [RelayCommand]
    private async Task OpenUsernamePasswordAsync(CancellationToken cancellationToken)
    {
        ContentDialogResult<UsernamePasswordViewModel> result = await _dialogService
            .CreateUsernamePasswordDialog("Supply a custom login")
            .ConfigureUsernameStep(
                "Enter username",
                username =>
                    new EmailAddressAttribute().IsValid(username)
                        ? ValidationResult.Success
                        : new ValidationResult("The Username should be a valid email address")
            )
            .ConfigurePasswordStep(
                "Enter password",
                checkHandler: async (username, password, token) =>
                {
                    await Task.Delay(1000, token);
                    return username == password;
                }
            )
            .ShowAsync(cancellationToken);
        if (!result.IsPrimary || !result.TryGetResultData(out UsernamePasswordData? x))
            return;
        await _dialogService.ShowMessageBoxDialogAsync(x.Username, x.Password, cancellationToken: cancellationToken);
    }
}
