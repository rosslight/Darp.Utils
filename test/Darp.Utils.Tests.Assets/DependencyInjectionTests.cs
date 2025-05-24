namespace Darp.Utils.Tests.Assets;

using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Utils.Assets;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddEmbeddedResourceAssetsService_ShouldNotThrow()
    {
        // Arrange
        ServiceProvider provider = new ServiceCollection()
            .AddEmbeddedResourceAssetsService<EmbeddedResourceAssetsServiceTests>()
            .BuildServiceProvider();

        // Act
        IEmbeddedResourceAssetsService service = provider.GetRequiredService<IEmbeddedResourceAssetsService>();

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
        IFolderAssetsService service = provider.GetRequiredService<IFolderAssetsService>();

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
        IAppDataAssetsService service = provider.GetRequiredService<IAppDataAssetsService>();

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
        IProgramDataAssetsService service = provider.GetRequiredService<IProgramDataAssetsService>();

        // Assert
        service.BasePath.ShouldBe($"{programDataPath}/{relativePath}");
    }

    [Fact]
    public void AddBaseDirectoryAssetsService_ShouldNotThrow()
    {
        // Arrange
        ServiceProvider provider = new ServiceCollection().AddBaseDirectoryAssetsService().BuildServiceProvider();

        // Act
        IBaseDirectoryAssetsService service = provider.GetRequiredService<IBaseDirectoryAssetsService>();

        // Assert
        service.BasePath.ShouldBe($"{AppDomain.CurrentDomain.BaseDirectory.Replace('\\', '/')}".TrimEnd('/'));
    }
}
