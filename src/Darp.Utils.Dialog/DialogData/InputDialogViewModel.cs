namespace Darp.Utils.Dialog.DialogData;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using static Helper.Strings;

/// <summary> Data for InputDialogs </summary>
public sealed class InputDialogViewModel : ObservableValidator, IDialogData<string>
{
    /// <summary> Initialize a new instance </summary>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = DynamicDependencyAddedForMethod)]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(InputDialogViewModel))]
#pragma warning disable CS0618 // Type or member is obsolete
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(__ObservableValidatorExtensions))]
#pragma warning restore CS0618 // Type or member is obsolete
    public InputDialogViewModel()
    {
        ValidateAllProperties();
    }

    /// <summary> The message to show on top of the <see cref="Input"/> Field </summary>
    public string? Message { get; init; }

    /// <summary> If set to true, the <see cref="Message"/> is selectable </summary>
    public bool IsMessageSelectable { get; init; }

    /// <summary> The input which can be set in the dialog </summary>
    [Required]
    [CustomValidation(typeof(InputDialogViewModel), nameof(ValidateInput))]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = DynamicDependencyAddedForMethod)]
    public string? Input
    {
        get;
        set => SetProperty(ref field, value, validate: true);
    }

    /// <summary> The watermark to show on the <see cref="Input"/> field </summary>
    public string? InputWatermark { get; internal set; }

    /// <summary> If set to true, the <see cref="Input"/> field is prepared for password input </summary>
    public bool IsPasswordInput { get; internal set; }

    /// <summary> Set a custom input validation </summary>
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
        if (string.IsNullOrEmpty(Input))
        {
            resultData = null;
            return false;
        }

        resultData = Input;
        return true;
    }
}
