namespace Darp.Utils.Tests.Assets;

using Shouldly;
using Utils.Assets;

public sealed class EmbeddedResourceAssetsServiceTests
{
    private readonly EmbeddedResourceAssetsService _service = new(typeof(EmbeddedResourceAssetsServiceTests).Assembly);

    [Fact]
    public async Task GetReadOnlySteam_WritingToFile_ShouldCreateFile()
    {
        // Arrange
        const string testFileName = "Assets/File1.txt";
        const string expectedContent = """
            TxtContent1

            """;

        // Act
        var content = await _service.ReadTextAsync(testFileName);

        // Assert
        content.ShouldBe(expectedContent);
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
        foundFiles.ShouldBe(expectedFiles);
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
        exists.ShouldBe(shouldExist);
    }
}
