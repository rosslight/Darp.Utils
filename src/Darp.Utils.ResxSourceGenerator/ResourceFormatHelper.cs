namespace Darp.Utils.ResxSourceGenerator;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

internal static class ResourceFormatHelper
{
    public static IReadOnlyList<string> GetArguments(string value, out bool usingNamedArgs)
    {
        return GetArguments(value, out usingNamedArgs, out _);
    }

    public static IReadOnlyList<string> GetArguments(string value, out bool usingNamedArgs, out bool hasMixedArguments)
    {
        var namedArguments = new List<string>();
        var numberedArguments = new List<string>();

        for (var i = 0; i < value.Length; i++)
        {
            if (value[i] != '{')
                continue;
            if (i + 1 < value.Length && value[i + 1] == '{')
            {
                i++;
                continue;
            }

            if (!TryReadFormatItem(value, i, out var argument, out var isNamed, out var end))
                continue;

            List<string> arguments = isNamed ? namedArguments : numberedArguments;
            if (!arguments.Contains(argument))
            {
                arguments.Add(argument);
            }

            i = end;
        }

        hasMixedArguments = namedArguments.Count > 0 && numberedArguments.Count > 0;
        if (hasMixedArguments)
        {
            usingNamedArgs = false;
            return [];
        }

        usingNamedArgs = namedArguments.Count > 0;
        if (usingNamedArgs)
        {
            return namedArguments;
        }

        if (numberedArguments.Count == 0)
        {
            return numberedArguments;
        }

        var maxArgumentIndex = numberedArguments.Select(x => Convert.ToInt32(x, CultureInfo.InvariantCulture)).Max();
        return Enumerable.Range(0, maxArgumentIndex + 1).Select(x => x.ToString(CultureInfo.InvariantCulture)).ToList();
    }

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

        var argumentEnd = argumentStart;
        if (char.IsDigit(value[argumentEnd]))
        {
            while (argumentEnd < value.Length && char.IsDigit(value[argumentEnd]))
            {
                argumentEnd++;
            }
        }
        else if (IsIdentifierStart(value[argumentEnd]))
        {
            isNamed = true;
            while (argumentEnd < value.Length && IsIdentifierPart(value[argumentEnd]))
            {
                argumentEnd++;
            }
        }
        else
        {
            return false;
        }

        closeBrace = argumentEnd;
        while (closeBrace < value.Length && value[closeBrace] != '}')
        {
            if (value[closeBrace] == '{')
            {
                closeBrace = -1;
                break;
            }

            closeBrace++;
        }

        if (closeBrace < 0 || closeBrace >= value.Length)
            return false;
        if (!IsValidFormatSuffix(value, argumentEnd, closeBrace))
            return false;

        argument = value.Substring(argumentStart, argumentEnd - argumentStart);
        return true;
    }

    private static bool IsValidFormatSuffix(string value, int start, int end)
    {
        if (start == end)
            return true;
        if (value[start] == ':')
            return true;
        if (value[start] != ',')
            return false;

        var i = start + 1;
        while (i < end && char.IsWhiteSpace(value[i]))
        {
            i++;
        }
        if (i < end && value[i] == '-')
        {
            i++;
        }

        var digitStart = i;
        while (i < end && char.IsDigit(value[i]))
        {
            i++;
        }

        if (i == digitStart)
            return false;
        if (i == end)
            return true;
        return value[i] == ':';
    }

    private static bool IsIdentifierStart(char c) => c is >= 'a' and <= 'z' or >= 'A' and <= 'Z';

    private static bool IsIdentifierPart(char c) => IsIdentifierStart(c) || c is (>= '0' and <= '9') or '_';
}
