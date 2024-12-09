namespace Darp.Utils.Tests.Assets;

using System.Reactive.Disposables;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Utils.Assets;

public sealed class FolderDataAssetsServiceTests
{
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
        appDataService.BasePath.Should().Be(joinedPath);
    }

    [Fact]
    public async Task GetWriteOnlySteam_WritingToFile_ShouldCreateFile()
    {
        // Arrange
        const string testFileName = "test.json";
        var testData = new TestData("Test");
        using IDisposable dis = CreateTemporaryFolderAssetsService(
            out IFolderAssetsService appDataService,
            out var _,
            out var _
        );

        // Act
        appDataService.Exists(testFileName).Should().BeFalse();
        await appDataService.SerializeJsonAsync(testFileName, testData);
        appDataService.Exists(testFileName).Should().BeTrue();
        TestData readData = await appDataService.DeserializeJsonAsync<TestData>(testFileName);

        // Assert
        readData.Should().BeEquivalentTo(testData);
    }

    [Fact]
    public async Task GetWriteOnlySteam_WritingToFile_ShouldReturnExistingFile()
    {
        // Arrange
        const string testFileName = "test.json";
        var testData = new TestData("Test");
        using IDisposable dis = CreateTemporaryFolderAssetsService(
            out IFolderAssetsService appDataService,
            out var _,
            out var _
        );

        // Act
        await appDataService.SerializeJsonAsync(testFileName, testData);
        await using Stream stream = appDataService.GetWriteOnlySteam(testFileName);

        stream.SetLength(0);
        await stream.WriteAsync(Array.Empty<byte>());
        Func<Task> act = async () => await appDataService.DeserializeJsonAsync<TestData>(testFileName);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public void UsingDI_AddFolderAssetsService_ShouldNotThrow()
    {
        // Arrange
        const string relativePath = "RelativePath";
        var appDataDirectory = Path.GetTempPath();
        ServiceProvider provider = new ServiceCollection()
            .AddFolderAssetsService(relativePath, appDataDirectory)
            .BuildServiceProvider();

        // Act
        Action act = () => provider.GetRequiredService<IFolderAssetsService>();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void UsingDI_AddAppDataAssetsService_ShouldNotThrow()
    {
        // Arrange
        const string relativePath = "RelativePath";
        ServiceProvider provider = new ServiceCollection().AddAppDataAssetsService(relativePath).BuildServiceProvider();

        // Act
        Action act = () => provider.GetRequiredService<IAppDataAssetsService>();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void UsingDI_AddProgramDataAssetsService_ShouldNotThrow()
    {
        // Arrange
        const string relativePath = "RelativePath";
        ServiceProvider provider = new ServiceCollection()
            .AddProgramDataAssetsService(relativePath)
            .BuildServiceProvider();

        // Act
        Action act = () => provider.GetRequiredService<IProgramDataAssetsService>();

        // Assert
        act.Should().NotThrow();
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
    public async Task EnumerateFiles_WritingToFile_ShouldCreateFile(string searchPattern, params string[] expectedFiles)
    {
        // Arrange
        const string dummyContent = "DummyContent";
        using IDisposable dis = CreateTemporaryFolderAssetsService(
            out IFolderAssetsService appDataService,
            out _,
            out _
        );

        // Act
        await appDataService.SerializeTextAsync(FileName1, dummyContent);
        await appDataService.SerializeTextAsync(FileName2, dummyContent);
        await appDataService.SerializeTextAsync(FileName3, dummyContent);
        await appDataService.SerializeTextAsync(FileName4, dummyContent);

        IEnumerable<string> foundFiles = appDataService.EnumerateFiles(searchPattern);

        // Assert
        foundFiles.Should().BeEquivalentTo(expectedFiles);
    }
}

internal sealed record TestData(string Prop1);
