namespace Darp.Utils.Dialog;

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

/// <summary> Wraps message box information </summary>
public sealed class MessageBoxModel : INotifyPropertyChanged
{
    private string _message;

    /// <summary> The message to be displayed </summary>
    public required string Message
    {
        get => _message;
        [MemberNotNull(nameof(_message))]
        set => SetField(ref _message, value);
    }

    /// <summary> If true, a selectable TextBlock will be used to show the message </summary>
    public bool IsSelectable { get; init; }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetField<T>([NotNullIfNotNull(nameof(value))] ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
