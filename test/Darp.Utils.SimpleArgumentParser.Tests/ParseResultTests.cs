namespace Darp.Utils.SimpleArgumentParser.Tests;

using Shouldly;
using Xunit;
using Parser = Darp.Utils.SimpleArgumentParser.SimpleArgumentParser;

public sealed class ParseResultTests
{
    [Fact]
    public void GetValue_ReturnsArgumentAndOptionalArgumentValues()
    {
        // Arrange
        var parser = new Parser();
        Argument<int> count = parser.AddRequiredNamed<int>("--count");
        OptionalArgument<string> name = parser.AddNamed<string>("--name", ParserTestHelpers.ParseString);

        // Act
        ParseResult result = parser.ShouldParseSuccessfully(["--count", "42", "--name", "alpha"]);

        // Assert
        result.GetValue(count).ShouldBe(42);
        result.GetValue(name).ShouldBe("alpha");
    }

    [Fact]
    public void GetValue_WhenOptionalArgumentIsAbsent_ReturnsNull()
    {
        // Arrange
        var parser = new Parser();
        OptionalArgument<string> name = parser.AddNamed<string>("--name", ParserTestHelpers.ParseString);

        // Act
        ParseResult result = parser.ShouldParseSuccessfully([]);

        // Assert
        result.GetValue(name).ShouldBeNull();
    }

    [Fact]
    public void GetValue_WhenArgumentIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var parser = new Parser();
        ParseResult result = parser.ShouldParseSuccessfully([]);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => result.GetValue((Argument<int>)null!));
        Should.Throw<ArgumentNullException>(() => result.GetValue((OptionalArgument<int>)null!));
    }

    [Fact]
    public void GetValue_WhenArgumentBelongsToAnotherParser_ThrowsArgumentException()
    {
        // Arrange
        var parser = new Parser();
        ParseResult result = parser.ShouldParseSuccessfully([]);

        var otherParser = new Parser();
        Argument<int> otherCount = otherParser.AddNamed("--count", 42);

        // Act & Assert
        Should.Throw<ArgumentException>(() => result.GetValue(otherCount));
    }

    [Fact]
    public void GetValue_WhenArgumentWasAddedAfterParse_ThrowsArgumentException()
    {
        // Arrange
        var parser = new Parser();
        ParseResult result = parser.ShouldParseSuccessfully([]);
        Argument<int> count = parser.AddNamed("--count", 42);

        // Act & Assert
        Should.Throw<ArgumentException>(() => result.GetValue(count));
    }
}
