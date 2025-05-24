namespace Darp.Utils.Tests.Assets;

using System.Reactive.Disposables;
using Shouldly;
using Shouldly.ShouldlyExtensionMethods;
using Utils.Assets;

public sealed class FolderDataAssetsServiceTests
{
    private static IDisposable CreateTemporaryFolderAssetsService(out IFolderAssetsService folderAssetsService)
    {
        return CreateTemporaryFolderAssetsService(out folderAssetsService, out _, out _);
    }

    private static IDisposable CreateTemporaryFolderAssetsService(
        out IFolderAssetsService folderAssetsService,
        out string tempDirectory,
        out string relativePath
    )
    {
        tempDirectory = Path.GetTempPath();
        relativePath = $"DarpUtils_{Guid.NewGuid()}";
        folderAssetsService = new FolderAssetsService(tempDirectory, relativePath);
        return Disposable.Create(folderAssetsService.BasePath, path => Directory.Delete(path, true));
    }

    [Fact]
    public void BasePath_FromConstructor_ShouldBeJoinedCorrectly()
    {
        // Arrange
        const string relativePath = "RelativePath";
        var tempDirectory = Path.GetTempPath();
        var joinedPath = Path.Join(tempDirectory, relativePath).Replace('\\', '/');

        // Act
        var appDataService = new FolderAssetsService(tempDirectory, relativePath);

        // Assert
        appDataService.BasePath.ShouldBe(joinedPath);
    }

    [Fact]
    public async Task GetWriteOnlySteam_WritingToFile_ShouldCreateFile()
    {
        // Arrange
        const string testFileName = "test.json";
        var testData = new TestData("Test");
        using IDisposable dis = CreateTemporaryFolderAssetsService(out IFolderAssetsService appDataService);

        // Act
        appDataService.Exists(testFileName).ShouldBeFalse();
        await appDataService.SerializeJsonAsync(testFileName, testData);
        appDataService.Exists(testFileName).ShouldBeTrue();
        TestData readData = await appDataService.DeserializeJsonAsync<TestData>(testFileName);

        // Assert
        readData.ShouldBe(testData);
    }

    [Fact]
    public async Task GetWriteOnlySteam_WritingToFile_ShouldReturnExistingFile()
    {
        // Arrange
        const string testFileName = "test.json";
        var testData = new TestData("Test");
        using IDisposable dis = CreateTemporaryFolderAssetsService(out IFolderAssetsService appDataService);

        // Act
        await appDataService.SerializeJsonAsync(testFileName, testData);
        await using Stream stream = appDataService.GetWriteOnlySteam(testFileName);

        stream.SetLength(0);
        await stream.WriteAsync(Array.Empty<byte>());
        Func<Task> act = async () => await appDataService.DeserializeJsonAsync<TestData>(testFileName);

        // Assert
        await act.ShouldThrowAsync<Exception>();
    }

    private const string FileName1 = "test1.json";
    private const string FileName2 = "test2.txt";
    private const string FileName3 = "testFolder/test3.json";
    private const string FileName4 = "test1/test4.txt";

    [Theory]
    [InlineData("*.json", FileName1)]
    [InlineData("*.txt", FileName2)]
    [InlineData("test1/*.json")]
    [InlineData("test1/*.txt", FileName4)]
    [InlineData("**/*.json", FileName1, FileName3)]
    public async Task EnumerateFiles(string searchPattern, params string[] expectedFiles)
    {
        // Arrange
        const string dummyContent = "DummyContent";
        using IDisposable dis = CreateTemporaryFolderAssetsService(out IFolderAssetsService appDataService);
        await appDataService.WriteTextAsync(FileName1, dummyContent);
        await appDataService.WriteTextAsync(FileName2, dummyContent);
        await appDataService.WriteTextAsync(FileName3, dummyContent);
        await appDataService.WriteTextAsync(FileName4, dummyContent);

        // Act
        IEnumerable<string> foundFiles = appDataService.EnumerateFiles(searchPattern);

        // Assert
        foundFiles.ShouldBe(expectedFiles);
    }

    [Fact]
    public async Task WritingToFile_WithAttributes_ShouldCreateFile()
    {
        // Arrange
        const string dummyContent = "DummyContent";
        using IDisposable dis = CreateTemporaryFolderAssetsService(out IFolderAssetsService appDataService);
        var file1Path = Path.Join(appDataService.BasePath, FileName1);

        // Act and Assert
        await appDataService.WriteTextAsync(FileName1, dummyContent, FileAttributes.ReadOnly);
        FileAttributes attributesAfterCreation = File.GetAttributes(file1Path);
        attributesAfterCreation.ShouldHaveFlag(FileAttributes.ReadOnly);
        await appDataService.WriteTextAsync(FileName1, dummyContent, FileAttributes.Normal);
        FileAttributes attributesAfterUpdate1 = File.GetAttributes(file1Path);
        attributesAfterUpdate1.ShouldNotHaveFlag(FileAttributes.ReadOnly);
    }
}

file sealed record TestData(string Prop1);
