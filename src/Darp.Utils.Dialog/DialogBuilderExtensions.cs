namespace Darp.Utils.Dialog;

using System.ComponentModel.DataAnnotations;
using DialogData;

/// <summary>/// Extensions for <see cref="IContentDialogBuilder{TContent}"/> </summary>
public static class DialogBuilderExtensions
{
    /// <summary> Configure the input field </summary>
    /// <param name="builder"> The input dialog builder </param>
    /// <param name="watermark"> An optional watermark on the input </param>
    /// <param name="isPassword"> If true, the input field is set up to hold a password </param>
    /// <param name="validateInput"> A validation callback called on input </param>
    /// <returns> The <see cref="IContentDialogBuilder{TContent}"/> </returns>
    public static IContentDialogBuilder<InputDialogData> ConfigureInput(
        this IContentDialogBuilder<InputDialogData> builder,
        string? watermark = null,
        bool? isPassword = null,
        Func<string?,ValidationResult?>? validateInput = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        if (watermark is not null)
        {
            builder.Content.InputWatermark = watermark;
        }
        if (isPassword is not null)
        {
            builder.Content.IsPasswordInput = isPassword.Value;
        }
        if (validateInput is not null)
        {
            builder.Content.ValidateInputCallback = validateInput;
        }
        return builder;
    }
}
