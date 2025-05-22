namespace Darp.Utils.Dialog.DialogData;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using static Helper.Strings;

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
public sealed partial class UsernamePasswordViewModel : ObservableValidator, IDialogData<UsernamePasswordData>
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
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = DynamicDependencyAddedForMethod)]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(UsernamePasswordViewModel))]
#pragma warning disable CS0618 // Type or member is obsolete
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(__ObservableValidatorExtensions))]
#pragma warning restore CS0618 // Type or member is obsolete
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
    [Required]
    [CustomValidation(typeof(UsernamePasswordViewModel), nameof(ValidateUsername))]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = DynamicDependencyAddedForMethod)]
    public string? Username
    {
        get;
        set => SetProperty(ref field, value, validate: true);
    }

    /// <summary> The password </summary>
    [Required]
    [CustomValidation(typeof(UsernamePasswordViewModel), nameof(ValidatePassword))]
    public string? Password
    {
        get;
        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = DynamicDependencyAddedForMethod)]
        set => SetProperty(ref field, value, validate: true);
    }

    /// <summary> An optional function which is used to check the validity of the password </summary>
    public ValidatePasswordAsync? CheckHandler { get; set; }

    /// <summary> The message </summary>
    public string? PasswordValidationError { get; private set; }

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
            foreach (var memberName in GetErrors().SelectMany(x => x.MemberNames))
            {
                return memberName switch
                {
                    nameof(Username) when Step is UsernamePasswordStep.RequestUsername => false,
                    nameof(Password) when Step is UsernamePasswordStep.RequestPassword => false,
                    _ => true,
                };
            }
            return true;
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
                if (string.IsNullOrWhiteSpace(Username))
                    return false;
                Step = UsernamePasswordStep.RequestPassword;
                return false;
            case UsernamePasswordStep.RequestPassword:
                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                    return false;
                if (
                    CheckHandler is not null
                    && !await CheckHandler(Username, Password, cancellationToken).ConfigureAwait(true)
                )
                {
                    PasswordValidationError = "Username or password are invalid";
                    OnPropertyChanged(nameof(PasswordValidationError));
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

    /// <summary> Clears the <see cref="PasswordValidationError"/> </summary>
    public void ClearPasswordValidationError()
    {
        PasswordValidationError = null;
        OnPropertyChanged(nameof(PasswordValidationError));
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
