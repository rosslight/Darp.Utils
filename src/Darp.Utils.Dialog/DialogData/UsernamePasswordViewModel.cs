namespace Darp.Utils.Dialog.DialogData;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Darp.MvvmValidator;

/// <summary> Current step of the <see cref="UsernamePasswordViewModel"/> dialog </summary>
public enum UsernamePasswordStep
{
    /// <summary> The dialog currently requests input of the username </summary>
    RequestUsername,

    /// <summary> The dialog currently requests input of the password </summary>
    RequestPassword,

    /// <summary> The dialog is done </summary>
    Done,
}

/// <summary> The data resulting of a call to the username password dialog </summary>
/// <param name="Username"> The value of <see cref="UsernamePasswordViewModel.Username"/> </param>
/// <param name="Password"> The value of <see cref="UsernamePasswordViewModel.Password"/> </param>
public sealed record UsernamePasswordData(string Username, string Password);

/// <summary> Wraps username password dialog information </summary>
public sealed partial class UsernamePasswordViewModel : ValidatingObservableObject, IDialogData<UsernamePasswordData>
{
    /// <summary> The delegate to validate a password </summary>
    /// <param name="username"> The username </param>
    /// <param name="password"> The password </param>
    /// <param name="cancellationToken"> The cancellation token to cancel the operation </param>
    /// <returns> True, if the password is valid, False otherwise </returns>
    public delegate Task<bool> ValidatePasswordAsync(
        string username,
        string password,
        CancellationToken cancellationToken
    );

    /// <summary> Initialize a new instance </summary>
    public UsernamePasswordViewModel()
    {
        ErrorsChanged += (_, _) => OnPropertyChanged(nameof(IsCurrentStepValid));
        ValidateAllProperties();
    }

    /// <summary> The message to be displayed when the username is requested </summary>
    [ObservableProperty]
    public partial string? EnterUsernameMessage { get; set; }

    /// <summary> The message to be displayed when the password is requested </summary>
    [ObservableProperty]
    public partial string? EnterPasswordMessage { get; set; }

    /// <summary> The username </summary>
    [ObservableProperty]
    [Required]
    [NotifyPropertyChangedFor(nameof(Password))] // Revalidates Password, which clears a failed check
    [CustomValidation(typeof(UsernamePasswordViewModel), nameof(ValidateUsername))]
    public partial string? Username { get; set; }

    /// <summary> The password </summary>
    [ObservableProperty]
    [Required]
    [CustomValidation(typeof(UsernamePasswordViewModel), nameof(ValidatePassword))]
    public partial string? Password { get; set; }

    /// <summary> An optional function which is used to check the validity of the password </summary>
    public ValidatePasswordAsync? CheckHandler { get; set; }

    /// <summary> The current step when entering a password </summary>
    public UsernamePasswordStep Step
    {
        get;
        private set
        {
            if (!SetProperty(ref field, value))
                return;
            OnPropertyChanged(nameof(IsUsernameSet));
            OnPropertyChanged(nameof(IsPasswordSet));
            OnPropertyChanged(nameof(IsCurrentStepValid));
        }
    } = UsernamePasswordStep.RequestUsername;

    /// <summary> Returns whether the username was set </summary>
    public bool IsUsernameSet => Step > UsernamePasswordStep.RequestUsername;

    /// <summary> Returns whether the password was set </summary>
    public bool IsPasswordSet => Step > UsernamePasswordStep.RequestPassword;

    /// <summary> Returns true if the current step is valid </summary>
    public bool IsCurrentStepValid
    {
        get
        {
            string propertyName;
            switch (Step)
            {
                case UsernamePasswordStep.RequestUsername:
                    propertyName = nameof(Username);
                    break;
                case UsernamePasswordStep.RequestPassword:
                    propertyName = nameof(Password);
                    break;
                case UsernamePasswordStep.Done:
                    return true;
                default:
                    throw new InvalidOperationException($"Unexpected username password step '{Step}'");
            }
            return !GetErrors(propertyName).Any();
        }
    }

    /// <summary> A func to validate the username on input </summary>
    public Func<string, ValidationResult?>? ValidateUsernameHandler { get; set; }

    /// <summary> A func to validate the password on input </summary>
    public Func<string, ValidationResult?>? ValidatePasswordHandler { get; set; }

    [RelayCommand]
    private void RequestUsernameStep()
    {
        if (Step is not UsernamePasswordStep.RequestPassword)
            return;
        Step = UsernamePasswordStep.RequestUsername;
    }

    internal async Task<bool> RequestNextStepAsync(CancellationToken cancellationToken)
    {
        switch (Step)
        {
            case UsernamePasswordStep.RequestUsername:
                if (!IsCurrentStepValid || string.IsNullOrWhiteSpace(Username))
                    return false;
                Step = UsernamePasswordStep.RequestPassword;
                return false;
            case UsernamePasswordStep.RequestPassword:
                if (!IsCurrentStepValid || string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                    return false;
                var password = Password;
                if (
                    CheckHandler is not null
                    && !await CheckHandler(Username, password, cancellationToken).ConfigureAwait(true)
                )
                {
                    // Skip the error if the password was edited while the check ran
                    if (Password == password)
                        SetErrors(nameof(Password), [new ValidationResult("Username or password are invalid")]);
                    return false;
                }
                Step = UsernamePasswordStep.Done;
                return true;
            case UsernamePasswordStep.Done:
            default:
                return true;
        }
    }

    /// <inheritdoc />
    public bool TryGetResultData([NotNullWhen(true)] out UsernamePasswordData? resultData)
    {
        if (Step is not UsernamePasswordStep.Done || string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
        {
            resultData = null;
            return false;
        }
        resultData = new UsernamePasswordData(Username, Password);
        return true;
    }

    /// <summary> Validate the username input. Necessary for the CustomValidation attribute </summary>
    /// <param name="username"> The input string </param>
    /// <param name="context"> The context of the validation </param>
    /// <returns> The ValidationResult. <see cref="ValidationResult.Success"/>, if username is null </returns>
    public static ValidationResult? ValidateUsername(string? username, ValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (username is null)
            return ValidationResult.Success;
        var instance = (UsernamePasswordViewModel)context.ObjectInstance;
        return instance.ValidateUsernameHandler?.Invoke(username) ?? ValidationResult.Success;
    }

    /// <summary> Validate the password input. Necessary for the CustomValidation attribute </summary>
    /// <param name="password"> The input string </param>
    /// <param name="context"> The context of the validation </param>
    /// <returns> The ValidationResult. <see cref="ValidationResult.Success"/>, if username is null </returns>
    public static ValidationResult? ValidatePassword(string? password, ValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (password is null)
            return ValidationResult.Success;
        var instance = (UsernamePasswordViewModel)context.ObjectInstance;
        return instance.ValidatePasswordHandler?.Invoke(password) ?? ValidationResult.Success;
    }
}
