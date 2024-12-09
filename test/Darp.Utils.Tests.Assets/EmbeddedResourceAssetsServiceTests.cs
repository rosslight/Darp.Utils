namespace Darp.Utils.Tests.Assets;

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Utils.Assets;

public sealed class EmbeddedResourceAssetsServiceTests
{
    private readonly IEmbeddedResourceAssetsService _service = new EmbeddedResourceAssetsService(
        typeof(EmbeddedResourceAssetsServiceTests).Assembly
    );

    [Fact]
    public async Task GetReadOnlySteam_WritingToFile_ShouldCreateFile()
    {
        // Arrange
        const string testFileName = "Assets/File1.txt";
        const string expectedContent = """
            TxtContent1

            """;

        // Act
        var content = await _service.DeserializeTextAsync(testFileName);

        // Assert
        content.Should().BeEquivalentTo(expectedContent);
    }

    private const string FileName1 = "Assets/File1.txt";
    private const string FileName2 = "Assets/File2.json";
    private const string FileName3 = "Assets/Folder1/File3.txt";
    private const string FileName4 = "Assets/Folder2/File4.json";

    [Theory]
    [InlineData("Assets/*.json", FileName2)]
    [InlineData("Assets/*.txt", FileName1)]
    [InlineData("Assets/Folder1/*.json")]
    [InlineData("Assets/Folder1/*.txt", FileName3)]
    [InlineData("**/*.json", FileName2, FileName4)]
    public void EnumerateFiles(string searchPattern, params string[] expectedFiles)
    {
        // Act
        IEnumerable<string> foundFiles = _service.EnumerateFiles(searchPattern);

        // Assert
        foundFiles.Should().BeEquivalentTo(expectedFiles);
    }

    [Theory]
    [InlineData("Assets/File1.txt", true)]
    [InlineData("Assets/File1.json", false)]
    [InlineData("File1.json", false)]
    [InlineData("Assets/Folder1/File3.txt", true)]
    [InlineData(null, false)]
    public void Exists(string? path, bool shouldExist)
    {
        // Act
        var exists = _service.Exists(path);

        // Assert
        exists.Should().Be(shouldExist);
    }
}
