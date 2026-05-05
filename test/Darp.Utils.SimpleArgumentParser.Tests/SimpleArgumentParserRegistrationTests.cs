namespace Darp.Utils.SimpleArgumentParser.Tests;

using Shouldly;
using Xunit;

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
        var parser = new ArgumentParser(description);

        // Assert
        parser.Description.ShouldBe(expectedDescription);
    }

    [Theory]
    [InlineData(" test option ", "test option")]
    [InlineData("   ", null)]
    public void AddNamed_NormalizesDescription(string description, string? expectedDescription)
    {
        // Arrange
        var parser = new ArgumentParser();

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
        var parser = new ArgumentParser();
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
    [InlineData("--count=value")]
    [InlineData("--count=")]
    [InlineData("--count value")]
    [InlineData("--count\tvalue")]
    public void AddFlag_WhenNameIsInvalid_ThrowsArgumentException(string? name)
    {
        // Arrange
        var parser = new ArgumentParser();

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
    [InlineData("--count=value")]
    [InlineData("--count=")]
    [InlineData("--count value")]
    [InlineData("--count\tvalue")]
    public void AddNamed_WhenNameIsInvalid_ThrowsArgumentException(string? name)
    {
        // Arrange
        var parser = new ArgumentParser();

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
    [InlineData("--count=value")]
    [InlineData("--count=")]
    [InlineData("--count value")]
    [InlineData("--count\tvalue")]
    public void AddRequiredNamed_WhenNameIsInvalid_ThrowsArgumentException(string? name)
    {
        // Arrange
        var parser = new ArgumentParser();

        // Act & Assert
        Should.Throw<ArgumentException>(() => parser.AddRequiredNamed<int>(name!));
    }

    [Theory]
    [InlineData("-value")]
    [InlineData("--value")]
    public void AddPositional_WhenNameStartsWithDash_ThrowsArgumentException(string name)
    {
        // Arrange
        var parser = new ArgumentParser();

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
        var parser = new ArgumentParser();

        // Act & Assert
        Should.Throw<ArgumentException>(() => parser.AddPositional<string>(name!, ParserTestHelpers.ParseString));
    }

    [Fact]
    public void AddRequiredPositional_WhenAddedAfterDefaultedPositional_ThrowsArgumentException()
    {
        // Arrange
        var parser = new ArgumentParser();
        parser.AddPositional("config", "default.cfg");

        // Act
        ArgumentException exception = Should.Throw<ArgumentException>(
            () => parser.AddRequiredPositional<string>("input", ParserTestHelpers.ParseString)
        );

        // Assert
        exception.Message.ShouldContain("Required positional argument 'input'");
        exception.Message.ShouldContain("optional or defaulted positional argument");
    }

    [Fact]
    public void AddRequiredPositional_WhenAddedAfterOptionalPositional_ThrowsArgumentException()
    {
        // Arrange
        var parser = new ArgumentParser();
        parser.AddPositional<string>("project", ParserTestHelpers.ParseString);

        // Act
        ArgumentException exception = Should.Throw<ArgumentException>(
            () => parser.AddRequiredPositional<string>("command", ParserTestHelpers.ParseString)
        );

        // Assert
        exception.Message.ShouldContain("Required positional argument 'command'");
        exception.Message.ShouldContain("optional or defaulted positional argument");
    }

    [Fact]
    public void AddNamed_WhenExactDuplicateOptionNameIsRegistered_ThrowsArgumentException()
    {
        // Arrange
        var parser = new ArgumentParser();
        parser.AddNamed<int>("--count");

        // Act & Assert
        Should.Throw<ArgumentException>(() => parser.AddFlag("--count"));
    }

    [Fact]
    public void AddNamed_WhenOptionNamesOnlyDifferByCase_RegistersSeparateArguments()
    {
        // Arrange
        var parser = new ArgumentParser();

        // Act
        OptionalArgument<int> lowerCount = parser.AddNamed<int>("--count");
        OptionalArgument<int> upperCount = parser.AddNamed<int>("--COUNT");

        // Assert
        lowerCount.Name.ShouldBe("count");
        upperCount.Name.ShouldBe("COUNT");
    }
}
