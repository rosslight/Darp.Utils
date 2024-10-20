namespace Darp.Utils.Dialog;

using System.Diagnostics.CodeAnalysis;
using DialogData;

/// <summary> Extensions for <see cref="ContentDialogResult{TContent}"/> </summary>
public static class ContentDialogResultExtensions
{
    /// <summary> Tries to get result data from a <see cref="ContentDialogResult{TContent}"/> </summary>
    /// <param name="dialogResult"> The content dialog result </param>
    /// <param name="resultData"> The dialog result data if return is True </param>
    /// <typeparam name="TContent"> The type of the dialog content </typeparam>
    /// <typeparam name="TResult"> The type of the result </typeparam>
    /// <returns> True, if the result data is present </returns>
    public static bool TryGetResultData<TContent, TResult>(
        this ContentDialogResult<TContent> dialogResult,
        [NotNullWhen(true)] out TResult? resultData
    )
        where TContent : IDialogData<TResult>
    {
        ArgumentNullException.ThrowIfNull(dialogResult);
        return dialogResult.Content.TryGetResultData(out resultData);
    }
}
