namespace Darp.Utils.Tests.Configuration;

using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Assets;
using Darp.Utils.Configuration;
using FluentAssertions;
using FluentAssertions.Events;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
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
            .AddMemoryAssetsService(BasePath)
            .AddConfigurationFile<TestConfig>("config.json")
            .BuildServiceProvider();

        IConfigurationService<TestConfig> service = provider.GetRequiredService<IConfigurationService<TestConfig>>();
        service.IsLoaded.Should().BeFalse();
    }

    [Fact]
    public void Config_WhenUsingDI_ShouldWorkWithContext()
    {
        // Arrange
        ServiceProvider provider = new ServiceCollection()
            .AddMemoryAssetsService(BasePath)
            .AddConfigurationFile<TestConfig>("config.json", TestConfigContext.Default.TestConfig)
            .BuildServiceProvider();

        IConfigurationService<TestConfig> service = provider.GetRequiredService<IConfigurationService<TestConfig>>();
        service.IsLoaded.Should().BeFalse();
    }

    [Fact]
    public void Config_WhenUsingDI_ShouldWorkWithMultipleAssets()
    {
        const string assetsName = "Memory2";
        // Arrange
        ServiceProvider provider = new ServiceCollection()
            .AddMemoryAssetsService("some/path")
            .AddMemoryAssetsService(assetsName, "some/other/path")
            .AddMemoryAssetsService("Memory3", "some/other/path/yea")
            .AddConfigurationFile<TestConfig>(assetsName, ConfigFileName, TestConfigContext.Default.TestConfig)
            .BuildServiceProvider();

        IConfigurationService<TestConfig> service = provider.GetRequiredService<IConfigurationService<TestConfig>>();
        service.IsLoaded.Should().BeFalse();
        service.Path.ShouldBe(Path.Join("some/other/path", ConfigFileName));
    }

    [Fact]
    public async Task Observe_WhenFirstChangeIsEnumDefault_ShouldEmitChangedValue()
    {
        // Arrange
        var assetsService = new MemoryAssetsService(BasePath);
        var configurationService = new ConfigurationService<TestConfig>(ConfigFileName, assetsService);
        await configurationService.WriteConfigurationAsync(
            new TestConfig { LogLevel = TestLogLevel.Warning },
            CancellationToken.None
        );
        var observer = new TestObserver<TestLogLevel>();

        // Act
        using IDisposable subscription = configurationService.Observe(x => x.LogLevel).Subscribe(observer);
        await configurationService.WriteConfigurationAsync(
            new TestConfig { LogLevel = TestLogLevel.Verbose },
            CancellationToken.None
        );

        // Assert
        observer.Values.Should().ContainSingle().Which.Should().Be(TestLogLevel.Verbose);
        configurationService.Dispose();
    }
}

internal sealed record TestConfig
{
    public string Setting { get; init; } = "Default";

    public TestLogLevel LogLevel { get; init; } = TestLogLevel.Verbose;
}

internal enum TestLogLevel
{
    Verbose = 0,
    Information = 1,
    Warning = 2,
}

internal sealed class TestObserver<T> : IObserver<T>
{
    public List<T> Values { get; } = [];

    public void OnCompleted() { }

    public void OnError(Exception error) => throw error;

    public void OnNext(T value) => Values.Add(value);
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(TestConfig))]
internal sealed partial class TestConfigContext : JsonSerializerContext;
