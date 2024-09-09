namespace Darp.Utils.Dialog.FluentAvalonia.DialogData;

using Avalonia.Controls;
using Avalonia.Data.Converters;
using Dialog.DialogData;

internal static class Converters
{
    public static readonly IValueConverter DialogCharConverter =
        new FuncValueConverter<bool, char>(b => b ? '*' : char.MinValue);
    public static readonly IValueConverter InputDialogClassesConverter =
        new FuncValueConverter<InputDialogData, Classes>(data => data?.IsPasswordInput is true
            ? new Classes("revealPasswordButton")
            : []);
}
