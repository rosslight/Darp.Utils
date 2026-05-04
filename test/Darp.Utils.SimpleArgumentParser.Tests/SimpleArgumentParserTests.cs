namespace Darp.Utils.SimpleArgumentParser.Tests;

using System;
using Shouldly;
using Xunit;
using Parser = Darp.Utils.SimpleArgumentParser.SimpleArgumentParser;

public sealed class SimpleArgumentParserTests
{
    [Theory]
    [InlineData(new[] { "--count", "42" }, 42)]
    [InlineData(new[] { "--count=42" }, 42)]
    public void TryParse_NamedArgument_ReturnsParsedValue(string[] args, int expectedValue)
    {
        // Arrange
        var parser = new Parser();
        OptionalArgument<int> count = parser.AddNamed<int>("count");

        // Act
        var success = parser.TryParse(args, out ParseResult? result, out string? error);

        // Assert
        success.ShouldBeTrue();
        result.ShouldNotBeNull();
        error.ShouldBeNull();
        result.GetValue(count).ShouldBe(expectedValue);
    }

    [Theory]
    [InlineData(new string[] { }, false)]
    [InlineData(new[] { "--verbose" }, true)]
    [InlineData(new[] { "--verbose=true" }, true)]
    [InlineData(new[] { "--verbose=false" }, false)]
    [InlineData(new[] { "--verbose", "true" }, true)]
    [InlineData(new[] { "--verbose", "false" }, false)]
    public void TryParse_Flag_ReturnsExpectedValue(string[] args, bool expectedValue)
    {
        // Arrange
        var parser = new Parser();
        Argument<bool> verbose = parser.AddFlag("verbose");

        // Act
        var success = parser.TryParse(args, out ParseResult? result, out string? error);

        // Assert
        success.ShouldBeTrue();
        result.ShouldNotBeNull();
        error.ShouldBeNull();
        result.GetValue(verbose).ShouldBe(expectedValue);
    }

    [Theory]
    [InlineData(DefaultArgumentKind.Named)]
    [InlineData(DefaultArgumentKind.Positional)]
    public void TryParse_DefaultArgument_ReturnsDefaultValue(DefaultArgumentKind kind)
    {
        // Arrange
        var parser = new Parser();
        Argument<int> count = kind switch
        {
            DefaultArgumentKind.Named => parser.AddNamed("count", 42),
            DefaultArgumentKind.Positional => parser.AddPositional("count", 42),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
        };

        // Act
        var success = parser.TryParse([], out ParseResult? result, out string? error);

        // Assert
        success.ShouldBeTrue();
        result.ShouldNotBeNull();
        error.ShouldBeNull();
        result.GetValue(count).ShouldBe(42);
    }

    [Fact]
    public void TryParse_Positionals_ReturnValuesInRegistrationOrder()
    {
        // Arrange
        var parser = new Parser();
        Argument<string> first = parser.AddRequiredPositional<string>("first", ParseString);
        Argument<int> second = parser.AddRequiredPositional<int>("second");

        // Act
        var success = parser.TryParse(["alpha", "42"], out ParseResult? result, out string? error);

        // Assert
        success.ShouldBeTrue();
        result.ShouldNotBeNull();
        error.ShouldBeNull();
        result.GetValue(first).ShouldBe("alpha");
        result.GetValue(second).ShouldBe(42);
    }

    [Fact]
    public void TryParse_StopParsingOptions_TreatsFollowingOptionTokenAsPositional()
    {
        // Arrange
        var parser = new Parser();
        Argument<string> value = parser.AddRequiredPositional<string>("value", ParseString);

        // Act
        var success = parser.TryParse(["--", "--value"], out ParseResult? result, out string? error);

        // Assert
        success.ShouldBeTrue();
        result.ShouldNotBeNull();
        error.ShouldBeNull();
        result.GetValue(value).ShouldBe("--value");
    }

    [Fact]
    public void TryParse_OptionalNamedArgument_WhenAbsent_ReturnsNull()
    {
        // Arrange
        var parser = new Parser();
        OptionalArgument<string> value = parser.AddNamed<string>("value", ParseString);

        // Act
        var success = parser.TryParse([], out ParseResult? result, out string? error);

        // Assert
        success.ShouldBeTrue();
        result.ShouldNotBeNull();
        error.ShouldBeNull();
        result.GetValue(value).ShouldBeNull();
    }

    [Theory]
    [InlineData(new string[] { }, null)]
    [InlineData(new[] { "alpha" }, "alpha")]
    public void TryParse_OptionalPositionalArgument_ReturnsExpectedValue(string[] args, string? expectedValue)
    {
        // Arrange
        var parser = new Parser();
        OptionalArgument<string> value = parser.AddPositional<string>("value", ParseString);

        // Act
        var success = parser.TryParse(args, out ParseResult? result, out string? error);

        // Assert
        success.ShouldBeTrue();
        result.ShouldNotBeNull();
        error.ShouldBeNull();
        result.GetValue(value).ShouldBe(expectedValue);
    }

    [Theory]
    [InlineData(FailureCase.UnknownOption, new[] { "--missing" }, "Unknown option '--missing'.")]
    [InlineData(FailureCase.MissingNamedValueAtEnd, new[] { "--count" }, "Option '--count' requires a value.")]
    [InlineData(
        FailureCase.MissingNamedValueBeforeOption,
        new[] { "--count", "--verbose" },
        "Option '--count' requires a value."
    )]
    [InlineData(
        FailureCase.InvalidNamedValue,
        new[] { "--count", "not-an-int" },
        "Argument 'count' has value 'not-an-int', which could not be parsed as Int32."
    )]
    [InlineData(
        FailureCase.InvalidPositionalValue,
        new[] { "not-an-int" },
        "Argument 'count' has value 'not-an-int', which could not be parsed as Int32."
    )]
    [InlineData(FailureCase.UnexpectedPositional, new[] { "extra" }, "Unexpected positional argument 'extra'.")]
    [InlineData(FailureCase.MissingRequiredPositional, new string[] { }, "Missing positional argument 'count'.")]
    public void TryParse_OnFailure_ReturnsErrorAndNullResult(
        FailureCase failureCase,
        string[] args,
        string expectedError
    )
    {
        // Arrange
        Parser parser = CreateParser(failureCase);

        // Act
        var success = parser.TryParse(args, out ParseResult? result, out string? error);

        // Assert
        success.ShouldBeFalse();
        result.ShouldBeNull();
        error.ShouldBe(expectedError);
    }

    private static Parser CreateParser(FailureCase failureCase)
    {
        var parser = new Parser();
        switch (failureCase)
        {
            case FailureCase.UnknownOption:
            case FailureCase.UnexpectedPositional:
                break;
            case FailureCase.MissingNamedValueAtEnd:
            case FailureCase.InvalidNamedValue:
                parser.AddRequiredNamed<int>("count");
                break;
            case FailureCase.MissingNamedValueBeforeOption:
                parser.AddRequiredNamed<int>("count");
                parser.AddFlag("verbose");
                break;
            case FailureCase.InvalidPositionalValue:
            case FailureCase.MissingRequiredPositional:
                parser.AddRequiredPositional<int>("count");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(failureCase), failureCase, null);
        }

        return parser;
    }

    private static bool ParseString(ReadOnlySpan<char> value, IFormatProvider? provider, out string result)
    {
        result = value.ToString();
        return true;
    }

    public enum DefaultArgumentKind
    {
        Named,
        Positional,
    }

    public enum FailureCase
    {
        UnknownOption,
        MissingNamedValueAtEnd,
        MissingNamedValueBeforeOption,
        InvalidNamedValue,
        InvalidPositionalValue,
        UnexpectedPositional,
        MissingRequiredPositional,
    }
}
