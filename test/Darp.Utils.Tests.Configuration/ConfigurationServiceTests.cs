namespace Darp.Utils.Tests.Configuration;

using System.IO;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Assets;
using Assets.Abstractions;
using Darp.Utils.Configuration;
using FluentAssertions;
using FluentAssertions.Events;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public sealed class ConfigurationServiceTests
{
    private const string ConfigFileName = "testConfig.json";
    private const string BasePath = "some/path";

    [Fact]
    public async Task LoadConfigurationAsync_FileExists_DeserializesAndReturnsConfig()
    {
        // Arrange
        var assetsService = new MemoryAssetsService(BasePath);
        var configurationService = new ConfigurationService<TestConfig>(ConfigFileName, assetsService);
        var expectedConfig = new TestConfig { Setting = "Test" };
        await assetsService.SerializeJsonAsync(ConfigFileName, expectedConfig);

        // Act
        TestConfig config = await configurationService.LoadConfigurationAsync(CancellationToken.None);
        configurationService.Dispose();

        // Assert
        config.Should().NotBeNull();
        config.Should().BeEquivalentTo(expectedConfig);
    }

    [Fact]
    public async Task LoadConfigurationAsync_FileDoesNotExist_CopiesFromDefaultAndLoads()
    {
        // Arrange
        var assetsService = new MemoryAssetsService(BasePath);
        var configurationService = new ConfigurationService<TestConfig>(ConfigFileName, assetsService);
        var expectedConfig = new TestConfig { Setting = "Default" };
        await assetsService.SerializeJsonAsync(ConfigFileName, expectedConfig);

        // Act
        TestConfig config = await configurationService.LoadConfigurationAsync(CancellationToken.None);
        configurationService.Dispose();

        // Assert
        config.Should().NotBeNull();
        config.Should().BeEquivalentTo(expectedConfig);
    }

    [Fact]
    public async Task WriteConfigurationAsync_WritesConfigAndUpdatesCachedConfig()
    {
        // Arrange
        var assetsService = new MemoryAssetsService(BasePath);
        var configurationService = new ConfigurationService<TestConfig>(ConfigFileName, assetsService);
        var newConfig = new TestConfig { Setting = "New" };

        // Act
        TestConfig returnedConfig = await configurationService.WriteConfigurationAsync(
            newConfig,
            CancellationToken.None
        );
        configurationService.Dispose();

        // Assert
        returnedConfig.Should().BeEquivalentTo(newConfig);
        configurationService.IsLoaded.Should().BeTrue();
        configurationService.Config.Should().BeEquivalentTo(newConfig);
        TestConfig writtenConfig = await assetsService.DeserializeJsonAsync<TestConfig>(ConfigFileName);
        writtenConfig.Should().Be(newConfig);
    }

    [Fact]
    public void Config_PropertyNotLoaded_ThrowsNullReferenceException()
    {
        // Act
        var assetsService = new MemoryAssetsService(BasePath);
        var configurationService = new ConfigurationService<TestConfig>(ConfigFileName, assetsService);
        configurationService.Dispose();

        // Assert
        configurationService.IsLoaded.Should().BeFalse();
        configurationService.Config.Should().BeEquivalentTo(new TestConfig());
    }

    [Fact]
    public void Config_Path_ReturnsCorrectPath()
    {
        // Arrange
        var assetsService = new MemoryAssetsService(BasePath);
        var configurationService = new ConfigurationService<TestConfig>(ConfigFileName, assetsService);

        // Assert
        configurationService.Path.Should().Be(Path.Join(BasePath, ConfigFileName));
        configurationService.Dispose();
    }

    [Fact]
    public async Task Config_PropertyChanged_ShouldNotRaiseOnSameWrite()
    {
        // Arrange
        var assetsService = new MemoryAssetsService(BasePath);
        var configurationService = new ConfigurationService<TestConfig>(ConfigFileName, assetsService);
        IMonitor<ConfigurationService<TestConfig>> monitor = configurationService.Monitor();

        // Act
        await configurationService.WriteConfigurationAsync(configurationService.Config, CancellationToken.None);
        configurationService.Dispose();

        // Assert
        monitor.Should().RaisePropertyChangeFor(service => service.IsLoaded);
        monitor.Should().NotRaisePropertyChangeFor(service => service.Config);
        monitor.Should().RaisePropertyChangeFor(service => service.IsDisposed);
    }

    [Fact]
    public async Task Config_PropertyChanged_ShouldRaiseOnDifferentWrite()
    {
        // Arrange
        var newConfig = new TestConfig { Setting = "newValue" };
        var assetsService = new MemoryAssetsService(BasePath);
        var configurationService = new ConfigurationService<TestConfig>(ConfigFileName, assetsService);
        IMonitor<ConfigurationService<TestConfig>> monitor = configurationService.Monitor();

        // Act
        await configurationService.WriteConfigurationAsync(newConfig, CancellationToken.None);
        configurationService.Dispose();

        // Assert
        monitor.Should().RaisePropertyChangeFor(service => service.IsLoaded);
        monitor.Should().RaisePropertyChangeFor(service => service.Config);
        monitor.Should().RaisePropertyChangeFor(service => service.IsDisposed);
    }

    [Fact]
    public void Config_WhenUsingDI_ShouldWork()
    {
        // Arrange
        ServiceProvider provider = new ServiceCollection()
            .AddSingleton<IAssetsService>(new MemoryAssetsService(BasePath))
            .AddConfigurationFile<TestConfig, IAssetsService>("config.json")
            .BuildServiceProvider();

        IConfigurationService<TestConfig> service = provider.GetRequiredService<IConfigurationService<TestConfig>>();
        service.IsLoaded.Should().BeFalse();
    }

    [Fact]
    public void Config_WhenUsingDI_ShouldWorkWithContext()
    {
        // Arrange
        ServiceProvider provider = new ServiceCollection()
            .AddSingleton<IAssetsService>(new MemoryAssetsService(BasePath))
            .AddConfigurationFile<TestConfig, IAssetsService>("config.json", TestConfigContext.Default.TestConfig)
            .BuildServiceProvider();

        IConfigurationService<TestConfig> service = provider.GetRequiredService<IConfigurationService<TestConfig>>();
        service.IsLoaded.Should().BeFalse();
    }
}

internal sealed record TestConfig
{
    public string Setting { get; init; } = "Default";
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(TestConfig))]
internal sealed partial class TestConfigContext : JsonSerializerContext;
