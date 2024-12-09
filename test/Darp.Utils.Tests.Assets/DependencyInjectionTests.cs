namespace Darp.Utils.Tests.Assets;

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
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
        service.BasePath.Should().Be("Darp/Utils/Tests/Assets");
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
        service.BasePath.Should().Be($"{tempFolder}/{relativePath}");
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
        service.BasePath.Should().Be($"{appDataPath}/{relativePath}");
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
        service.BasePath.Should().Be($"{programDataPath}/{relativePath}");
    }

    [Fact]
    public void AddBaseDirectoryAssetsService_ShouldNotThrow()
    {
        // Arrange
        ServiceProvider provider = new ServiceCollection().AddBaseDirectoryAssetsService().BuildServiceProvider();

        // Act
        IBaseDirectoryAssetsService service = provider.GetRequiredService<IBaseDirectoryAssetsService>();

        // Assert
        service.BasePath.Should().Be($"{AppDomain.CurrentDomain.BaseDirectory.Replace('\\', '/')}");
    }
}
