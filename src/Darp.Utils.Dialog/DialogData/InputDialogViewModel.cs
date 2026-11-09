namespace Darp.Utils.Dialog.DialogData;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using Darp.MvvmValidator;

/// <summary> Data for InputDialogs </summary>
public sealed partial class InputDialogViewModel : ValidatingObservableObject, IDialogData<string>
{
    /// <summary> The message to show on top of the <see cref="Input"/> Field </summary>
    public string? Message { get; init; }

    /// <summary> If set to true, the <see cref="Message"/> is selectable </summary>
    public bool IsMessageSelectable { get; init; }

    /// <summary>
    /// The input which can be set in the dialog. It is validated on every change, but not before the first one,
    /// so an untouched dialog shows no error
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsInputValid))]
    [CustomValidation(typeof(InputDialogViewModel), nameof(ValidateInput))]
    public partial string? Input { get; set; }

    /// <summary> True, if the <see cref="Input"/> is not empty or whitespace and passed validation </summary>
    [MemberNotNullWhen(true, nameof(Input))]
    internal bool IsInputValid => !HasErrors && !string.IsNullOrWhiteSpace(Input);

    /// <summary> The watermark to show on the <see cref="Input"/> field </summary>
    public string? InputWatermark { get; internal set; }

    /// <summary> If set to true, the <see cref="Input"/> field is prepared for password input </summary>
    public bool IsPasswordInput { get; internal set; }

    /// <summary>
    /// Set a custom input validation. Its errors are the only ones shown. Empty or whitespace input is never accepted,
    /// so return an error for it to tell the user why
    /// </summary>
    public Func<string?, ValidationResult?>? ValidateInputCallback { get; set; }

    /// <summary> Validate the input. Necessary for the CustomValidation attribute </summary>
    /// <param name="input"> The input string </param>
    /// <param name="context"> The context of the validation </param>
    /// <returns> The ValidationResult </returns>
    public static ValidationResult? ValidateInput(string? input, ValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var instance = (InputDialogViewModel)context.ObjectInstance;
        return instance.ValidateInputCallback?.Invoke(input) ?? ValidationResult.Success;
    }

    /// <inheritdoc />
    public bool TryGetResultData([NotNullWhen(true)] out string? resultData)
    {
        if (!IsInputValid)
        {
            resultData = null;
            return false;
        }

        resultData = Input;
        return true;
    }
}
