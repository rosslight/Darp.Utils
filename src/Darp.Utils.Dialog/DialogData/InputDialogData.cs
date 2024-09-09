namespace Darp.Utils.Dialog.DialogData;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;

/// <summary> Data for InputDialogs </summary>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public sealed partial class InputDialogData : ObservableValidator, IDialogData
{
    /// <summary> Initialize a new instance </summary>
    [RequiresUnreferencedCode("This method requires the generated CommunityToolkit. Mvvm. ComponentModel.__Internals.__ObservableValidatorExtensions type not to be removed to use the fast path")]
    public InputDialogData()
    {
        ValidateAllProperties();
    }

    /// <summary> The message to show on top of the <see cref="Input"/> Field </summary>
    public string? Message { get; init; }
    /// <summary> If set to true, the <see cref="Message"/> is selectable </summary>
    public bool IsMessageSelectable { get; init; }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    [CustomValidation(typeof(InputDialogData), nameof(ValidateInput))]
    private string? _input;
    /// <summary> The watermark to show on the <see cref="Input"/> field </summary>
    public string? InputWatermark { get; internal set; }
    /// <summary> If set to true, the <see cref="Input"/> field is prepared for password input </summary>
    public bool IsPasswordInput { get; internal set; }
    /// <summary> Set a custom input validation </summary>
    public Func<string?, ValidationResult?>? ValidateInputCallback { get; set; }

    private static ValidationResult? ValidateInput(string? input, ValidationContext context)
    {
        var instance = (InputDialogData)context.ObjectInstance;
        return instance.ValidateInputCallback?.Invoke(input);
    }
}
