namespace Darp.Utils.ResxSourceGenerator;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

internal enum FormatArgumentStyle
{
    /// <summary> The value contains no format items </summary>
    None,

    /// <summary> The value contains only numbered format items like <c>{0}</c> </summary>
    Numbered,

    /// <summary> The value contains only named format items like <c>{name}</c> </summary>
    Named,

    /// <summary> The value contains both numbered and named format items </summary>
    Mixed,

    /// <summary> The value contains a numbered format item above <see cref="ResourceFormatHelper.MaxArgumentIndex"/> </summary>
    NumberedOutOfRange,
}

internal static class ResourceFormatHelper
{
    /// <summary> The highest index of a numbered format item a format method is generated for </summary>
    /// <remarks> Every index up to the highest one becomes a parameter, so an unbounded index would generate an unbounded method </remarks>
    public const int MaxArgumentIndex = 255;

    /// <summary> Find the arguments of all composite format items in the <paramref name="value"/> </summary>
    /// <param name="value"> The resource value to search </param>
    /// <param name="style"> The style of the format items found </param>
    /// <returns>
    /// The names in order of first appearance for <see cref="FormatArgumentStyle.Named"/>,
    /// every index from 0 to the highest one for <see cref="FormatArgumentStyle.Numbered"/> and empty otherwise
    /// </returns>
    public static IReadOnlyList<string> GetArguments(string value, out FormatArgumentStyle style)
    {
        var namedArguments = new List<string>();
        var maxArgumentIndex = -1;
        var hasIndexOutOfRange = false;

        for (var i = 0; i < value.Length; i++)
        {
            if (value[i] != '{')
                continue;
            if (i + 1 < value.Length && value[i + 1] == '{')
            {
                i++;
                continue;
            }

            if (!TryReadFormatItem(value, i, out var argument, out var isNamed, out var closeBrace))
                continue;

            if (isNamed)
            {
                if (!namedArguments.Contains(argument))
                    namedArguments.Add(argument);
            }
            else if (
                int.TryParse(argument, NumberStyles.None, CultureInfo.InvariantCulture, out var index)
                && index <= MaxArgumentIndex
            )
            {
                maxArgumentIndex = Math.Max(maxArgumentIndex, index);
            }
            else
            {
                hasIndexOutOfRange = true;
            }

            i = closeBrace;
        }

        var hasNumberedArguments = maxArgumentIndex >= 0 || hasIndexOutOfRange;
        if (namedArguments.Count > 0)
        {
            style = hasNumberedArguments ? FormatArgumentStyle.Mixed : FormatArgumentStyle.Named;
            return hasNumberedArguments ? [] : namedArguments;
        }
        if (hasIndexOutOfRange)
        {
            style = FormatArgumentStyle.NumberedOutOfRange;
            return [];
        }
        if (maxArgumentIndex < 0)
        {
            style = FormatArgumentStyle.None;
            return [];
        }

        style = FormatArgumentStyle.Numbered;
        return Enumerable.Range(0, maxArgumentIndex + 1).Select(x => x.ToString(CultureInfo.InvariantCulture)).ToList();
    }

    /// <summary> Read a format item of the form <c>{argument[,alignment][:formatString]}</c> </summary>
    private static bool TryReadFormatItem(
        string value,
        int openBrace,
        out string argument,
        out bool isNamed,
        out int closeBrace
    )
    {
        argument = "";
        isNamed = false;
        closeBrace = -1;

        var argumentStart = openBrace + 1;
        if (argumentStart >= value.Length)
            return false;

        var i = argumentStart;
        if (IsAsciiDigit(value[i]))
        {
            while (i < value.Length && IsAsciiDigit(value[i]))
                i++;
        }
        else if (value[i].IsIdentifierStartCharacter())
        {
            isNamed = true;
            while (i < value.Length && value[i].IsIdentifierPartCharacter())
                i++;
        }
        else
        {
            return false;
        }
        var argumentEnd = i;

        i = SkipSpaces(value, i);
        if (i < value.Length && value[i] == ',')
        {
            i = SkipSpaces(value, i + 1);
            if (i < value.Length && value[i] == '-')
                i++;

            var widthStart = i;
            while (i < value.Length && IsAsciiDigit(value[i]))
                i++;
            if (i == widthStart)
                return false;
            i = SkipSpaces(value, i);
        }

        // The format string is not validated. Its meaning depends on the type of the argument
        if (i < value.Length && value[i] == ':')
        {
            while (i < value.Length && value[i] != '}' && value[i] != '{')
                i++;
        }

        if (i >= value.Length || value[i] != '}')
            return false;

        argument = value.Substring(argumentStart, argumentEnd - argumentStart);
        closeBrace = i;
        return true;
    }

    private static int SkipSpaces(string value, int start)
    {
        while (start < value.Length && value[start] == ' ')
            start++;
        return start;
    }

    private static bool IsAsciiDigit(char c) => c is >= '0' and <= '9';
}
