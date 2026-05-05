namespace Darp.Utils.SimpleArgumentParser;

/// <summary> A class that provides simple argument parsers </summary>
public static class SimpleArgumentParsers
{
    /// <summary>
    /// Parses an enum value using the same delegate shape as parser registrations.
    /// </summary>
    /// <param name="value">The token text to parse.</param>
    /// <param name="_">Ignored format provider.</param>
    /// <param name="result">The parsed enum value when parsing succeeds.</param>
    /// <typeparam name="T">The enum value type.</typeparam>
    /// <returns><see langword="true"/> when the value matched an enum name or numeric value.</returns>
    public static bool TryParseEnum<T>(ReadOnlySpan<char> value, IFormatProvider? _, out T result)
        where T : struct, Enum => Enum.TryParse(value, out result);

    /// <summary>
    /// Parses an enum value ignoring the case using the same delegate shape as parser registrations.
    /// </summary>
    /// <param name="value">The token text to parse.</param>
    /// <param name="_">Ignored format provider.</param>
    /// <param name="result">The parsed enum value when parsing succeeds.</param>
    /// <typeparam name="T">The enum value type.</typeparam>
    /// <returns><see langword="true"/> when the value matched an enum name or numeric value.</returns>
    public static bool TryParseEnumIgnoreCase<T>(ReadOnlySpan<char> value, IFormatProvider? _, out T result)
        where T : struct, Enum => Enum.TryParse(value, ignoreCase: true, out result);
}
