namespace Darp.Utils.Tests.Assets;

using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Utils.Assets;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void Add_ShouldNotThrow()
    {
        // Arrange
        ServiceProvider provider = new ServiceCollection()
            .AddEmbeddedResourceAssetsService<EmbeddedResourceAssetsServiceTests>()
            .BuildServiceProvider();

        // Act
        IAssetsFactory factory = provider.GetRequiredService<IAssetsFactory>();
        IReadOnlyAssetsService service = factory.GetReadOnlyAssets();

        // Assert
        service.BasePath.ShouldBe("Darp/Utils/Tests/Assets");
    }

    [Fact]
    public void AddEmbeddedResourceAssetsService_ShouldNotThrow()
    {
        // Arrange
        ServiceProvider provider = new ServiceCollection()
            .AddEmbeddedResourceAssetsService<EmbeddedResourceAssetsServiceTests>()
            .BuildServiceProvider();

        // Act
        EmbeddedResourceAssetsService service = provider.GetRequiredService<EmbeddedResourceAssetsService>();

        // Assert
        service.BasePath.ShouldBe("Darp/Utils/Tests/Assets");
    }

    [Fact]
    public void AddFolderAssetsService_ShouldNotThrow()
    {
        // Arrange
        const string relativePath = "RelativePath";
        var tempFolder = Path.GetTempPath();
        ServiceProvider provider = new ServiceCollection()
            .AddFolderAssetsService(tempFolder, relativePath)
            .BuildServiceProvider();
        tempFolder = tempFolder.Replace('\\', '/').TrimEnd('/');

        // Act
        FolderAssetsService service = provider.GetRequiredService<FolderAssetsService>();

        // Assert
        service.BasePath.ShouldBe($"{tempFolder}/{relativePath}");
    }

    [Fact]
    public void AddAppDataAssetsService_ShouldNotThrow()
    {
        // Arrange
        const string relativePath = "RelativePath";
        ServiceProvider provider = new ServiceCollection().AddAppDataAssetsService(relativePath).BuildServiceProvider();
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace('\\', '/');

        // Act
        FolderAssetsService service = provider.GetRequiredService<FolderAssetsService>();

        // Assert
        service.BasePath.ShouldBe($"{appDataPath}/{relativePath}");
    }

    [Fact]
    public void AddProgramDataAssetsService_ShouldNotThrow()
    {
        // Arrange
        const string relativePath = "RelativePath";
        ServiceProvider provider = new ServiceCollection()
            .AddProgramDataAssetsService(relativePath)
            .BuildServiceProvider();
        var programDataPath = Environment
            .GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
            .Replace('\\', '/');

        // Act
        FolderAssetsService service = provider.GetRequiredService<FolderAssetsService>();

        // Assert
        service.BasePath.ShouldBe($"{programDataPath}/{relativePath}");
    }

    [Fact]
    public void AddBaseDirectoryAssetsService_ShouldNotThrow()
    {
        // Arrange
        ServiceProvider provider = new ServiceCollection().AddBaseDirectoryAssetsService().BuildServiceProvider();

        // Act
        IReadOnlyAssetsService service = provider.GetRequiredService<IReadOnlyAssetsService>();

        // Assert
        service.BasePath.ShouldBe($"{AppDomain.CurrentDomain.BaseDirectory.Replace('\\', '/')}".TrimEnd('/'));
    }
}
