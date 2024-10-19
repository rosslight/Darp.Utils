namespace Darp.Utils.Dialog.DialogData;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

/// <summary> Current state of the <see cref="UsernamePasswordViewModel"/> dialog </summary>
public enum UsernamePasswordState
{
    /// <summary> The dialog currently requests input of the username </summary>
    RequestUsername,

    /// <summary> The dialog currently requests input of the password </summary>
    RequestPassword,

    /// <summary> The dialog is done </summary>
    Done,
}

/// <summary> Wraps username password dialog information </summary>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public partial class UsernamePasswordViewModel : ObservableValidator, IDialogData
{
    /// <summary> Initialize a new instance </summary>
    [RequiresUnreferencedCode(
        "This method requires the generated CommunityToolkit. Mvvm. ComponentModel.__Internals.__ObservableValidatorExtensions type not to be removed to use the fast path"
    )]
    public UsernamePasswordViewModel()
    {
        ErrorsChanged += (_, _) => OnPropertyChanged(nameof(IsCurrentStateValid));
        ValidateAllProperties();
    }

    /// <summary> The delegate defining stuff </summary>
    public delegate Task<bool> CheckPassword(string username, string password);

    /// <summary> The message to be displayed when the username is requested </summary>
    [ObservableProperty]
    private string? _enterUsernameMessage;

    /// <summary> The message to be displayed when the password is requested </summary>
    [ObservableProperty]
    private string? _enterPasswordMessage;

    private UsernamePasswordState _state = UsernamePasswordState.RequestUsername;

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

    /// <summary> Returns true if the current state is valid </summary>
    public bool IsCurrentStateValid
    {
        get
        {
            foreach (var memberName in GetErrors().SelectMany(x => x.MemberNames))
            {
                return memberName switch
                {
                    nameof(Username) when State is UsernamePasswordState.RequestUsername => false,
                    nameof(Password) when State is UsernamePasswordState.RequestPassword => false,
                    _ => true,
                };
            }
            return true;
        }
    }

    /// <summary> Returns whether the username was set </summary>
    public bool IsUsernameSet => State > UsernamePasswordState.RequestUsername;

    /// <summary> Returns whether the password was set </summary>
    public bool IsPasswordSet => State > UsernamePasswordState.RequestPassword;

    /// <summary> An optional function which is used to check the validity of the password </summary>
    public CheckPassword? CheckPasswordHandler { get; set; }

    /// <summary> The current step when entering a password </summary>
    public UsernamePasswordState State
    {
        get => _state;
        set
        {
            if (!SetProperty(ref _state, value))
                return;
            OnPropertyChanged(nameof(IsUsernameSet));
            OnPropertyChanged(nameof(IsPasswordSet));
            OnPropertyChanged(nameof(IsCurrentStateValid));
        }
    }

    [RelayCommand]
    private void RequestUsernameStep()
    {
        if (State is not UsernamePasswordState.RequestPassword)
        {
            return;
        }
        State = UsernamePasswordState.RequestUsername;
    }

    internal async Task<bool> RequestNextStepAsync(CancellationToken cancellationToken)
    {
        switch (State)
        {
            case UsernamePasswordState.RequestUsername:
                if (string.IsNullOrWhiteSpace(Username))
                    return false;
                State = UsernamePasswordState.RequestPassword;
                return false;
            case UsernamePasswordState.RequestPassword:
                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                    return false;
                if (
                    CheckPasswordHandler is not null
                    && !await CheckPasswordHandler(Username, Password).ConfigureAwait(true)
                )
                {
                    return false;
                }
                State = UsernamePasswordState.Done;
                return true;

            case UsernamePasswordState.Done:
            default:
                return true;
        }
    }
}
