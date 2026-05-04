namespace Darp.Utils.SimpleArgumentParser.Tests;

using Shouldly;
using Xunit;
using Parser = Darp.Utils.SimpleArgumentParser.SimpleArgumentParser;

public sealed class SimpleArgumentParserRegistrationTests
{
    [Theory]
    [InlineData(" test parser ", "test parser")]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData(null, null)]
    public void Constructor_NormalizesDescription(string? description, string? expectedDescription)
    {
        // Arrange & Act
        var parser = new Parser(description);

        // Assert
        parser.Description.ShouldBe(expectedDescription);
    }

    [Theory]
    [InlineData(" test option ", "test option")]
    [InlineData("   ", null)]
    public void AddNamed_NormalizesDescription(string description, string? expectedDescription)
    {
        // Arrange
        var parser = new Parser();

        // Act
        OptionalArgument<int> argument = parser.AddNamed<int>("--count", description);

        // Assert
        argument.Description.ShouldBe(expectedDescription);
    }

    [Theory]
    [InlineData("--count")]
    [InlineData("  --count  ")]
    public void AddNamed_NormalizesOptionName(string registeredName)
    {
        // Arrange
        var parser = new Parser();
        OptionalArgument<int> count = parser.AddNamed<int>(registeredName);

        // Act
        ParseResult result = parser.ShouldParseSuccessfully(["--count", "42"]);

        // Assert
        count.Name.ShouldBe("count");
        result.GetValue(count).ShouldBe(42);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("verbose")]
    [InlineData("-v")]
    [InlineData("-verbose")]
    [InlineData("-")]
    [InlineData("--")]
    [InlineData("---verbose")]
    public void AddFlag_WhenNameIsInvalid_ThrowsArgumentException(string? name)
    {
        // Arrange
        var parser = new Parser();

        // Act & Assert
        Should.Throw<ArgumentException>(() => parser.AddFlag(name!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("count")]
    [InlineData("-")]
    [InlineData("--")]
    [InlineData("-count")]
    [InlineData("---count")]
    public void AddNamed_WhenNameIsInvalid_ThrowsArgumentException(string? name)
    {
        // Arrange
        var parser = new Parser();

        // Act & Assert
        Should.Throw<ArgumentException>(() => parser.AddNamed<int>(name!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("count")]
    [InlineData("-")]
    [InlineData("--")]
    [InlineData("-count")]
    [InlineData("---count")]
    public void AddRequiredNamed_WhenNameIsInvalid_ThrowsArgumentException(string? name)
    {
        // Arrange
        var parser = new Parser();

        // Act & Assert
        Should.Throw<ArgumentException>(() => parser.AddRequiredNamed<int>(name!));
    }

    [Theory]
    [InlineData("-value")]
    [InlineData("--value")]
    public void AddPositional_WhenNameStartsWithDash_ThrowsArgumentException(string name)
    {
        // Arrange
        var parser = new Parser();

        // Act & Assert
        Should.Throw<ArgumentException>(() => parser.AddPositional<string>(name, ParserTestHelpers.ParseString));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddPositional_WhenNameIsEmpty_ThrowsArgumentException(string? name)
    {
        // Arrange
        var parser = new Parser();

        // Act & Assert
        Should.Throw<ArgumentException>(() => parser.AddPositional<string>(name!, ParserTestHelpers.ParseString));
    }

    [Fact]
    public void AddNamed_WhenDuplicateOptionNameIsRegistered_ThrowsArgumentException()
    {
        // Arrange
        var parser = new Parser();
        parser.AddNamed<int>("--count");

        // Act & Assert
        Should.Throw<ArgumentException>(() => parser.AddFlag("--COUNT"));
    }
}
