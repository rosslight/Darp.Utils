namespace Darp.Utils.SimpleArgumentParser.Tests;

using Shouldly;
using Xunit;
using Parser = Darp.Utils.SimpleArgumentParser.SimpleArgumentParser;

public sealed class SimpleArgumentParserTryParseTests
{
    [Theory]
    [InlineData(new[] { "--count", "42" }, 42)]
    [InlineData(new[] { "--count=42" }, 42)]
    public void TryParse_NamedArgument_ReturnsParsedValue(string[] args, int expectedValue)
    {
        // Arrange
        var parser = new Parser();
        OptionalArgument<int> count = parser.AddNamed<int>("--count");

        // Act
        ParseResult result = parser.ShouldParseSuccessfully(args);

        // Assert
        result.GetValue(count).ShouldBe(expectedValue);
    }

    [Fact]
    public void TryParse_NamedArgument_MatchesOptionCaseSensitively()
    {
        // Arrange
        var parser = new Parser();
        OptionalArgument<int> lowerCount = parser.AddNamed<int>("--count");
        OptionalArgument<int> upperCount = parser.AddNamed<int>("--COUNT");

        // Act
        ParseResult result = parser.ShouldParseSuccessfully(["--count", "42", "--COUNT", "100"]);

        // Assert
        result.GetValue(lowerCount).ShouldBe(42);
        result.GetValue(upperCount).ShouldBe(100);
    }

    [Theory]
    [InlineData(new[] { "--verbose", "true" }, true)]
    [InlineData(new[] { "--verbose=false" }, false)]
    public void TryParse_NamedBoolArgument_ReturnsParsedValue(string[] args, bool expectedValue)
    {
        // Arrange
        var parser = new Parser();
        OptionalArgument<bool> verbose = parser.AddNamed<bool>("--verbose");

        // Act
        ParseResult result = parser.ShouldParseSuccessfully(args);

        // Assert
        result.GetValue(verbose).ShouldBe(expectedValue);
    }

    [Theory]
    [InlineData(new string[] { }, 42)]
    [InlineData(new[] { "--count", "100" }, 100)]
    public void TryParse_DefaultNamedArgument_ReturnsExpectedValue(string[] args, int expectedValue)
    {
        // Arrange
        var parser = new Parser();
        Argument<int> count = parser.AddNamed("--count", 42);

        // Act
        ParseResult result = parser.ShouldParseSuccessfully(args);

        // Assert
        result.GetValue(count).ShouldBe(expectedValue);
    }

    [Theory]
    [InlineData(new string[] { }, 42)]
    [InlineData(new[] { "100" }, 100)]
    public void TryParse_DefaultPositionalArgument_ReturnsExpectedValue(string[] args, int expectedValue)
    {
        // Arrange
        var parser = new Parser();
        Argument<int> count = parser.AddPositional("count", 42);

        // Act
        ParseResult result = parser.ShouldParseSuccessfully(args);

        // Assert
        result.GetValue(count).ShouldBe(expectedValue);
    }

    [Fact]
    public void TryParse_RequiredNamedArgument_ReturnsParsedValue()
    {
        // Arrange
        var parser = new Parser();
        Argument<int> count = parser.AddRequiredNamed<int>("--count");

        // Act
        ParseResult result = parser.ShouldParseSuccessfully(["--count", "42"]);

        // Assert
        result.GetValue(count).ShouldBe(42);
    }

    [Fact]
    public void TryParse_RequiredPositionalArgument_ReturnsParsedValue()
    {
        // Arrange
        var parser = new Parser();
        Argument<int> count = parser.AddRequiredPositional<int>("count");

        // Act
        ParseResult result = parser.ShouldParseSuccessfully(["42"]);

        // Assert
        result.GetValue(count).ShouldBe(42);
    }

    [Fact]
    public void TryParse_RequiredPositionalFollowedByDefaultedPositional_WithOneToken_UsesDefault()
    {
        // Arrange
        var parser = new Parser();
        Argument<string> input = parser.AddRequiredPositional<string>("input", ParserTestHelpers.ParseString);
        Argument<string> config = parser.AddPositional("config", ParserTestHelpers.ParseString, "default.cfg");

        // Act
        ParseResult result = parser.ShouldParseSuccessfully(["input.txt"]);

        // Assert
        result.GetValue(input).ShouldBe("input.txt");
        result.GetValue(config).ShouldBe("default.cfg");
    }

    [Fact]
    public void TryParse_RequiredPositionalFollowedByOptionalPositional_WithOneToken_ReturnsNullForOptional()
    {
        // Arrange
        var parser = new Parser();
        Argument<string> source = parser.AddRequiredPositional<string>("source", ParserTestHelpers.ParseString);
        OptionalArgument<string> destination = parser.AddPositional<string>(
            "destination",
            ParserTestHelpers.ParseString
        );

        // Act
        ParseResult result = parser.ShouldParseSuccessfully(["source.txt"]);

        // Assert
        result.GetValue(source).ShouldBe("source.txt");
        result.GetValue(destination).ShouldBeNull();
    }

    [Fact]
    public void TryParse_OptionalNamedArgument_WhenAbsent_ReturnsNull()
    {
        // Arrange
        var parser = new Parser();
        OptionalArgument<string> value = parser.AddNamed<string>("--value", ParserTestHelpers.ParseString);

        // Act
        ParseResult result = parser.ShouldParseSuccessfully([]);

        // Assert
        result.GetValue(value).ShouldBeNull();
    }

    [Fact]
    public void TryParse_OptionalNamedValueTypeArgument_WhenAbsent_ReturnsNull()
    {
        // Arrange
        var parser = new Parser();
        OptionalArgument<int> value = parser.AddNamed<int>("--value");

        // Act
        ParseResult result = parser.ShouldParseSuccessfully([]);

        // Assert
        result.GetValue(value).ShouldBeNull();
    }

    [Fact]
    public void TryParse_OptionalPositionalArgument_WhenAbsent_ReturnsNull()
    {
        // Arrange
        var parser = new Parser();
        OptionalArgument<string> value = parser.AddPositional<string>("value", ParserTestHelpers.ParseString);

        // Act
        ParseResult result = parser.ShouldParseSuccessfully([]);

        // Assert
        result.GetValue(value).ShouldBeNull();
    }

    [Fact]
    public void TryParse_OptionalPositionalValueTypeArgument_WhenAbsent_ReturnsNull()
    {
        // Arrange
        var parser = new Parser();
        OptionalArgument<int> value = parser.AddPositional<int>("value");

        // Act
        ParseResult result = parser.ShouldParseSuccessfully([]);

        // Assert
        result.GetValue(value).ShouldBeNull();
    }

    [Theory]
    [InlineData(new string[] { }, false)]
    [InlineData(new[] { "--verbose" }, true)]
    public void TryParse_Flag_ReturnsExpectedValue(string[] args, bool expectedValue)
    {
        // Arrange
        var parser = new Parser();
        Argument<bool> verbose = parser.AddFlag("--verbose");

        // Act
        ParseResult result = parser.ShouldParseSuccessfully(args);

        // Assert
        result.GetValue(verbose).ShouldBe(expectedValue);
    }

    [Fact]
    public void TryParse_FlagFollowedByToken_DoesNotConsumePositionalToken()
    {
        // Arrange
        var parser = new Parser();
        Argument<bool> verbose = parser.AddFlag("--verbose");
        Argument<string> path = parser.AddRequiredPositional<string>("path", ParserTestHelpers.ParseString);

        // Act
        ParseResult result = parser.ShouldParseSuccessfully(["--verbose", "file.txt"]);

        // Assert
        result.GetValue(verbose).ShouldBeTrue();
        result.GetValue(path).ShouldBe("file.txt");
    }

    [Fact]
    public void TryParse_Positionals_ReturnValuesInRegistrationOrder()
    {
        // Arrange
        var parser = new Parser();
        Argument<string> first = parser.AddRequiredPositional<string>("first", ParserTestHelpers.ParseString);
        Argument<int> second = parser.AddRequiredPositional<int>("second");

        // Act
        ParseResult result = parser.ShouldParseSuccessfully(["alpha", "42"]);

        // Assert
        result.GetValue(first).ShouldBe("alpha");
        result.GetValue(second).ShouldBe(42);
    }

    [Theory]
    [InlineData("--value")]
    [InlineData("-x")]
    public void TryParse_StopParsingOptions_TreatsFollowingOptionTokenAsPositional(string token)
    {
        // Arrange
        var parser = new Parser();
        Argument<string> value = parser.AddRequiredPositional<string>("value", ParserTestHelpers.ParseString);

        // Act
        ParseResult result = parser.ShouldParseSuccessfully(["--", token]);

        // Assert
        result.GetValue(value).ShouldBe(token);
    }

    [Fact]
    public void TryParse_MixedNamedAndPositionalArguments_ReturnsExpectedValues()
    {
        // Arrange
        var parser = new Parser();
        Argument<string> input = parser.AddRequiredPositional<string>("input", ParserTestHelpers.ParseString);
        OptionalArgument<int> count = parser.AddNamed<int>("--count");
        Argument<bool> verbose = parser.AddFlag("--verbose");
        Argument<string> output = parser.AddRequiredPositional<string>("output", ParserTestHelpers.ParseString);

        // Act
        ParseResult result = parser.ShouldParseSuccessfully(["--count", "42", "input.txt", "--verbose", "output.txt"]);

        // Assert
        result.GetValue(input).ShouldBe("input.txt");
        result.GetValue(count).ShouldBe(42);
        result.GetValue(verbose).ShouldBeTrue();
        result.GetValue(output).ShouldBe("output.txt");
    }

    [Fact]
    public void TryParse_WhenOptionIsUnknown_ReturnsError()
    {
        // Arrange
        var parser = new Parser();

        // Act & Assert
        parser.ShouldFailWith(["--missing"], "Unknown option '--missing'.");
    }

    [Fact]
    public void TryParse_WhenOptionCaseDoesNotMatchRegistration_ReturnsUnknownOptionError()
    {
        // Arrange
        var parser = new Parser();
        parser.AddNamed<int>("--count");

        // Act & Assert
        parser.ShouldFailWith(["--COUNT", "42"], "Unknown option '--COUNT'.");
    }

    [Theory]
    [InlineData("-x")]
    [InlineData("-count")]
    [InlineData("-x=value")]
    public void TryParse_WhenTokenUsesUnsupportedShortOption_ReturnsUnknownOptionError(string arg)
    {
        // Arrange
        var parser = new Parser();

        // Act & Assert
        parser.ShouldFailWith([arg], $"Unknown option '{arg}'.");
    }

    [Fact]
    public void TryParse_WhenShortOptionTokenCouldBePositional_ReturnsUnknownOptionError()
    {
        // Arrange
        var parser = new Parser();
        parser.AddRequiredPositional<string>("path", ParserTestHelpers.ParseString);

        // Act & Assert
        parser.ShouldFailWith(["-x"], "Unknown option '-x'.");
    }

    [Fact]
    public void TryParse_WhenNamedValueIsUnsupportedShortOption_ReturnsMissingValueError()
    {
        // Arrange
        var parser = new Parser();
        parser.AddRequiredNamed<int>("--count");

        // Act & Assert
        parser.ShouldFailWith(["--count", "-x"], "Option '--count' requires a value.");
    }

    [Fact]
    public void TryParse_NamedArgument_AcceptsNegativeNumberValue()
    {
        // Arrange
        var parser = new Parser();
        Argument<int> count = parser.AddRequiredNamed<int>("--count");

        // Act
        ParseResult result = parser.ShouldParseSuccessfully(["--count", "-1"]);

        // Assert
        result.GetValue(count).ShouldBe(-1);
    }

    [Fact]
    public void TryParse_NamedArgument_AcceptsNegativeDecimalValue()
    {
        // Arrange
        var parser = new Parser();
        Argument<double> amount = parser.AddRequiredNamed<double>("--amount");

        // Act
        ParseResult result = parser.ShouldParseSuccessfully(["--amount", "-1.5"]);

        // Assert
        result.GetValue(amount).ShouldBe(-1.5);
    }

    [Fact]
    public void TryParse_PositionalArgument_AcceptsNegativeNumberValue()
    {
        // Arrange
        var parser = new Parser();
        Argument<int> count = parser.AddRequiredPositional<int>("count");

        // Act
        ParseResult result = parser.ShouldParseSuccessfully(["-1"]);

        // Assert
        result.GetValue(count).ShouldBe(-1);
    }

    [Fact]
    public void TryParse_PositionalArgument_AcceptsSingleDashValue()
    {
        // Arrange
        var parser = new Parser();
        Argument<string> value = parser.AddRequiredPositional<string>("value", ParserTestHelpers.ParseString);

        // Act
        ParseResult result = parser.ShouldParseSuccessfully(["-"]);

        // Assert
        result.GetValue(value).ShouldBe("-");
    }

    [Fact]
    public void TryParse_WhenNamedValueIsMissingAtEnd_ReturnsError()
    {
        // Arrange
        var parser = new Parser();
        parser.AddRequiredNamed<int>("--count");

        // Act & Assert
        parser.ShouldFailWith(["--count"], "Option '--count' requires a value.");
    }

    [Fact]
    public void TryParse_WhenNamedValueIsMissingBeforeOption_ReturnsError()
    {
        // Arrange
        var parser = new Parser();
        parser.AddRequiredNamed<int>("--count");
        parser.AddFlag("--verbose");

        // Act & Assert
        parser.ShouldFailWith(["--count", "--verbose"], "Option '--count' requires a value.");
    }

    [Fact]
    public void TryParse_WhenRequiredNamedArgumentIsMissing_ReturnsError()
    {
        // Arrange
        var parser = new Parser();
        parser.AddRequiredNamed<int>("--count");

        // Act & Assert
        parser.ShouldFailWith([], "Missing option '--count'.");
    }

    [Fact]
    public void TryParse_WhenRequiredPositionalArgumentIsMissing_ReturnsError()
    {
        // Arrange
        var parser = new Parser();
        parser.AddRequiredPositional<int>("count");

        // Act & Assert
        parser.ShouldFailWith([], "Missing positional argument 'count'.");
    }

    [Fact]
    public void TryParse_WhenNamedValueIsInvalid_ReturnsError()
    {
        // Arrange
        var parser = new Parser();
        parser.AddRequiredNamed<int>("--count");

        // Act & Assert
        parser.ShouldFailWith(
            ["--count", "not-an-int"],
            "Argument 'count' has value 'not-an-int', which could not be parsed as Int32."
        );
    }

    [Fact]
    public void TryParse_WhenPositionalValueIsInvalid_ReturnsError()
    {
        // Arrange
        var parser = new Parser();
        parser.AddRequiredPositional<int>("count");

        // Act & Assert
        parser.ShouldFailWith(
            ["not-an-int"],
            "Argument 'count' has value 'not-an-int', which could not be parsed as Int32."
        );
    }

    [Theory]
    [InlineData("--verbose=true")]
    [InlineData("--verbose=false")]
    [InlineData("--verbose=maybe")]
    public void TryParse_WhenFlagHasExplicitValue_ReturnsError(string arg)
    {
        // Arrange
        var parser = new Parser();
        parser.AddFlag("--verbose");

        // Act & Assert
        parser.ShouldFailWith([arg], "Option '--verbose' does not accept a value.");
    }

    [Fact]
    public void TryParse_WhenFlagIsFollowedByUnexpectedValue_ReturnsUnexpectedPositionalError()
    {
        // Arrange
        var parser = new Parser();
        parser.AddFlag("--verbose");

        // Act & Assert
        parser.ShouldFailWith(["--verbose", "false"], "Unexpected positional argument 'false'.");
    }

    [Fact]
    public void TryParse_WhenPositionalArgumentIsUnexpected_ReturnsError()
    {
        // Arrange
        var parser = new Parser();

        // Act & Assert
        parser.ShouldFailWith(["extra"], "Unexpected positional argument 'extra'.");
    }
}
