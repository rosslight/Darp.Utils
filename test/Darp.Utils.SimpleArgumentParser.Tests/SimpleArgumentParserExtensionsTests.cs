namespace Darp.Utils.SimpleArgumentParser.Tests;

using Shouldly;
using Xunit;
using Parser = Darp.Utils.SimpleArgumentParser.SimpleArgumentParser;

public sealed class SimpleArgumentParserExtensionsTests
{
    [Fact]
    public void AddNamedOptionalOverload_UsesISpanParsableParser()
    {
        // Arrange
        var parser = new Parser();
        OptionalArgument<int> count = parser.AddNamed<int>("--count");

        // Act
        ParseResult result = parser.ShouldParseSuccessfully(["--count", "42"]);

        // Assert
        result.GetValue(count).ShouldBe(42);
    }

    [Fact]
    public void AddRequiredNamedOverload_UsesISpanParsableParser()
    {
        // Arrange
        var parser = new Parser();
        Argument<Guid> id = parser.AddRequiredNamed<Guid>("--id");

        // Act
        ParseResult result = parser.ShouldParseSuccessfully(["--id", "00000000-0000-0000-0000-000000000000"]);

        // Assert
        result.GetValue(id).ShouldBe(Guid.Empty);
    }

    [Fact]
    public void AddNamedDefaultOverload_UsesISpanParsableParser()
    {
        // Arrange
        var parser = new Parser();
        Argument<bool> enabled = parser.AddNamed("--enabled", false);

        // Act
        ParseResult result = parser.ShouldParseSuccessfully(["--enabled", "true"]);

        // Assert
        result.GetValue(enabled).ShouldBeTrue();
    }

    [Fact]
    public void AddRequiredPositionalOverload_UsesISpanParsableParser()
    {
        // Arrange
        var parser = new Parser();
        Argument<DateTimeOffset> timestamp = parser.AddRequiredPositional<DateTimeOffset>("timestamp");

        // Act
        ParseResult result = parser.ShouldParseSuccessfully(["1/1/0001 12:00:00 AM +00:00"]);

        // Assert
        result.GetValue(timestamp).ShouldBe(DateTimeOffset.MinValue);
    }

    [Fact]
    public void AddPositionalOptionalOverload_UsesISpanParsableParser()
    {
        // Arrange
        var parser = new Parser();
        OptionalArgument<string> value = parser.AddPositional<string>("value");

        // Act
        ParseResult result = parser.ShouldParseSuccessfully(["alpha"]);

        // Assert
        result.GetValue(value).ShouldBe("alpha");
    }

    [Fact]
    public void AddPositionalDefaultOverload_UsesISpanParsableParser()
    {
        // Arrange
        var parser = new Parser();
        Argument<int> count = parser.AddPositional("count", 42);

        // Act
        ParseResult result = parser.ShouldParseSuccessfully(["100"]);

        // Assert
        result.GetValue(count).ShouldBe(100);
    }

    [Fact]
    public void TryParseEnum_WhenValueMatchesEnumName_ReturnsTrue()
    {
        // Act
        var success = SimpleArgumentParsers.TryParseEnum("X2", null, out SampleChoice result);

        // Assert
        success.ShouldBeTrue();
        result.ShouldBe(SampleChoice.X2);
    }

    [Fact]
    public void TryParseEnumIgnoreCase_WhenValueMatchesEnumName_ReturnsTrue()
    {
        // Act
        var success = SimpleArgumentParsers.TryParseEnumIgnoreCase("x2", null, out SampleChoice result);

        // Assert
        success.ShouldBeTrue();
        result.ShouldBe(SampleChoice.X2);
    }

    [Fact]
    public void ParseOrExit_WhenParseSucceeds_ReturnsResult()
    {
        // Arrange
        var parser = new Parser();
        Argument<int> count = parser.AddRequiredPositional<int>("count");

        // Act
        ParseResult result = parser.ParseOrExit(["42"]);

        // Assert
        result.GetValue(count).ShouldBe(42);
    }

    private enum SampleChoice
    {
        X2 = 42,
    }
}
