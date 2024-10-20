namespace Darp.Utils.Dialog.DialogData;

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

/// <summary> An abstract interface to identify all DialogData ViewModels </summary>
public interface IDialogData : INotifyPropertyChanged, INotifyPropertyChanging;

/// <summary> An abstract interface to mark dialogs with a specific result </summary>
/// <typeparam name="TResult"> The type of the result </typeparam>
public interface IDialogData<TResult> : IDialogData
{
    /// <summary> Tries to get a result </summary>
    /// <param name="resultData"> The dialog result if return is True </param>
    /// <returns> True, if the result data is present </returns>
    bool TryGetResultData([NotNullWhen(true)] out TResult? resultData);
}
