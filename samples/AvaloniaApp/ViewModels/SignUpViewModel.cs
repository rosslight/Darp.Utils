namespace AvaloniaApp.ViewModels;

using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Darp.MvvmValidator;

/// <summary> A form validated by source generated code, without reflection </summary>
public sealed partial class SignUpViewModel : ValidatingObservableObject
{
    [ObservableProperty]
    [Required]
    [StringLength(20, MinimumLength = 3)]
    [Display(Name = "User name")]
    public partial string? UserName { get; set; }

    [ObservableProperty]
    [Required]
    [EmailAddress]
    public partial string? Email { get; set; }

    [ObservableProperty]
    [Required]
    [MinLength(8)]
    public partial string? Password { get; set; }

    [ObservableProperty]
    [Compare(nameof(Password))]
    [Display(Name = "Repeated password")]
    public partial string? ConfirmPassword { get; set; }

    [ObservableProperty]
    public partial string? Result { get; set; }

    [RelayCommand]
    private void Submit()
    {
        ValidateAllProperties();
        Result = HasErrors ? $"{GetErrors().Count()} error(s)" : $"Signed up {UserName}";
    }
}
