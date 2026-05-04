namespace Darp.Utils.SimpleArgumentParser.Tests;

using Shouldly;
using Parser = Darp.Utils.SimpleArgumentParser.SimpleArgumentParser;

internal static class ParserTestHelpers
{
    internal static ParseResult ShouldParseSuccessfully(this Parser parser, string[] args)
    {
        var success = parser.TryParse(args, out ParseResult? result, out string? error);

        success.ShouldBeTrue(error);
        result.ShouldNotBeNull();
        error.ShouldBeNull();
        return result;
    }

    internal static void ShouldFailWith(this Parser parser, string[] args, string expectedError)
    {
        var success = parser.TryParse(args, out ParseResult? result, out string? error);

        success.ShouldBeFalse();
        result.ShouldBeNull();
        error.ShouldBe(expectedError);
    }

    internal static bool ParseString(ReadOnlySpan<char> value, IFormatProvider? _, out string result)
    {
        result = value.ToString();
        return true;
    }
}
