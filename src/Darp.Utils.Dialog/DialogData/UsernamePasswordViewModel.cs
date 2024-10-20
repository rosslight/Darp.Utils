namespace Darp.Utils.Dialog.DialogData;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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
public record UsernamePasswordData(string Username, string Password);

/// <summary> Wraps username password dialog information </summary>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public partial class UsernamePasswordViewModel : ObservableValidator, IDialogData<UsernamePasswordData>
{
    /// <summary> The delegate to validate a password </summary>
    /// <param name="username"> The username </param>
    /// <param name="password"> The password </param>
    /// <param name="cancellationToken"> The cancellation token to cancel the operation </param>
    /// <returns> True, if password is valid, False otherwise </returns>
    public delegate Task<bool> ValidatePassword(string username, string password, CancellationToken cancellationToken);

    private UsernamePasswordStep _step = UsernamePasswordStep.RequestUsername;

    /// <summary> The message to be displayed when the username is requested </summary>
    [ObservableProperty]
    private string? _enterUsernameMessage;

    /// <summary> The message to be displayed when the password is requested </summary>
    [ObservableProperty]
    private string? _enterPasswordMessage;

    /// <summary> The username </summary>
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    private string? _username;

    /// <summary> The username </summary>
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    private string? _password;

    /// <summary> Initialize a new instance </summary>
    [RequiresUnreferencedCode(
        "This method requires the generated CommunityToolkit. Mvvm. ComponentModel.__Internals.__ObservableValidatorExtensions type not to be removed to use the fast path"
    )]
    public UsernamePasswordViewModel()
    {
        ErrorsChanged += (_, _) => OnPropertyChanged(nameof(IsCurrentStepValid));
        ValidateAllProperties();
    }

    /// <summary> An optional function which is used to check the validity of the password </summary>
    public ValidatePassword? CheckPasswordHandler { get; set; }

    /// <summary> The message </summary>
    public string? PasswordValidationError { get; private set; }

    /// <summary> The current step when entering a password </summary>
    public UsernamePasswordStep Step
    {
        get => _step;
        set
        {
            if (!SetProperty(ref _step, value))
                return;
            OnPropertyChanged(nameof(IsUsernameSet));
            OnPropertyChanged(nameof(IsPasswordSet));
            OnPropertyChanged(nameof(IsCurrentStepValid));
        }
    }

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

    [RelayCommand]
    private void RequestUsernameStep()
    {
        if (Step is not UsernamePasswordStep.RequestPassword)
        {
            return;
        }
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
                    CheckPasswordHandler is not null
                    && !await CheckPasswordHandler(Username, Password, cancellationToken).ConfigureAwait(true)
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
            resultData = default;
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
}
