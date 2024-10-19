namespace Darp.Utils.Dialog.FluentAvalonia.DialogData;

using Avalonia.Data.Converters;

internal static class Converters
{
    public static readonly IValueConverter DialogCharConverter = new FuncValueConverter<bool, char>(b =>
        b ? '*' : char.MinValue
    );
}
