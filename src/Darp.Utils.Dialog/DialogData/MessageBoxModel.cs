namespace Darp.Utils.Dialog.DialogData;

using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;

/// <summary> Wraps message box information </summary>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public sealed class MessageBoxModel : ObservableObject, IDialogData
{
    private string _message;

    /// <summary> The message to be displayed </summary>
    public required string Message
    {
        get => _message;
        [MemberNotNull(nameof(_message))]
        set => SetProperty(ref _message, value);
    }

    /// <summary> If true, a selectable TextBlock will be used to show the message </summary>
    public bool IsSelectable { get; init; }
}
