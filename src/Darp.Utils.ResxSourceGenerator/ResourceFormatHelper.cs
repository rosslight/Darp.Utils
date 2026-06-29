namespace Darp.Utils.ResxSourceGenerator;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

internal static class ResourceFormatHelper
{
    private static readonly Regex NamedParameterMatcher = new(
        @"(?<!\{)\{([a-z]\w*)(?:,\s*-?\d+)?(?::[^{}]*)?\}(?!\})",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );
    private static readonly Regex NumberParameterMatcher = new(
        @"(?<!\{)\{(\d+)(?:,\s*-?\d+)?(?::[^{}]*)?\}(?!\})",
        RegexOptions.Compiled
    );

    public static IReadOnlyList<string> GetArguments(string value, out bool usingNamedArgs)
    {
        MatchCollection match = NamedParameterMatcher.Matches(value);
        usingNamedArgs = match.Count > 0;

        if (!usingNamedArgs)
        {
            match = NumberParameterMatcher.Matches(value);
        }

        IEnumerable<string> arguments = match.Cast<Match>().Select(m => m.Groups[1].Value).Distinct();
        if (!usingNamedArgs)
        {
            arguments = arguments.OrderBy(Convert.ToInt32);
        }

        return arguments.ToList();
    }

    public static string ReplaceNamedFormatItems(string value, IReadOnlyList<string>? formatterNames)
    {
        if (formatterNames is null)
            return value;

        for (var i = 0; i < formatterNames.Count; i++)
        {
            var pattern = @"(?<!\{)\{" + Regex.Escape(formatterNames[i]) + @"(,\s*-?\d+)?(:[^{}]*)?\}(?!\})";
            value = Regex.Replace(value, pattern, match => $"{{{i}{match.Groups[1].Value}{match.Groups[2].Value}}}");
        }

        return value;
    }
}
