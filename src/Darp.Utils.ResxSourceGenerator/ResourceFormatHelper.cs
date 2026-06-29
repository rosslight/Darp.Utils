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
}
