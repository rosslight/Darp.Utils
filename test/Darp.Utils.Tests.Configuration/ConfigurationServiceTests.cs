namespace Darp.Utils.Tests.Configuration;

using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Assets;
using Darp.Utils.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

public sealed class ConfigurationServiceTests
{
    private const string ConfigFileName = "testConfig.json";
    private const string BasePath = "some/path";

    public static CancellationToken CancellationToken { get; } = TestContext.Current.CancellationToken;

    [Fact]
    public async Task LoadConfigAsync_FileExists_DeserializesAndReturnsConfig()
    {
        // Arrange
        var assetsService = new MemoryAssetsService(BasePath);
        var configurationService = new ConfigService<TestConfig>(ConfigFileName, assetsService);
        var expectedConfig = new TestConfig { Setting = "Test" };
        await assetsService.SerializeJsonAsync(ConfigFileName, expectedConfig, cancellationToken: CancellationToken);

        // Act
        TestConfig config = await configurationService.LoadConfigAsync(
            () => throw new ShouldAssertException("Initial config provider should not be called"),
            CancellationToken
        );
        configurationService.Dispose();

        // Assert
        config.ShouldNotBeNull();
        config.ShouldBeEquivalentTo(expectedConfig);
    }

    [Fact]
    public async Task LoadConfigAsync_FileDoesNotExist_CreatesAndLoadsInitialConfig()
    {
        // Arrange
        var assetsService = new MemoryAssetsService(BasePath);
        var configurationService = new ConfigService<TestConfig>(ConfigFileName, assetsService);
        var expectedConfig = new TestConfig { Setting = "Default" };
        // Act
        TestConfig config = await configurationService.LoadConfigAsync(() => expectedConfig, CancellationToken);
        configurationService.Dispose();

        // Assert
        config.ShouldNotBeNull();
        config.ShouldBeEquivalentTo(expectedConfig);
        TestConfig writtenConfig = await assetsService.DeserializeJsonAsync<TestConfig>(
            ConfigFileName,
            cancellationToken: CancellationToken
        );
        writtenConfig.ShouldBe(expectedConfig);
    }

    [Fact]
    public async Task LoadConfigAsync_FileDoesNotExist_WhenInitialConfigProviderReturnsNull_ShouldThrowAndNotCreateFile()
    {
        // Arrange
        var assetsService = new MemoryAssetsService(BasePath);
        var configurationService = new ConfigService<TestConfig>(ConfigFileName, assetsService);

        // Act
        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await configurationService.LoadConfigAsync(() => null!, CancellationToken)
        );
        configurationService.Dispose();

        // Assert
        configurationService.IsLoaded.ShouldBeFalse();
        assetsService.Exists(ConfigFileName).ShouldBeFalse();
    }

    [Fact]
    public async Task LoadConfigAsync_WhenFirstLoad_ShouldRaiseIsLoadedThenConfigChanged()
    {
        // Arrange
        var assetsService = new MemoryAssetsService(BasePath);
        var configurationService = new ConfigService<TestConfig>(ConfigFileName, assetsService);
        List<string?> events = [];
        configurationService.PropertyChanged += (_, args) => events.Add(args.PropertyName);

        // Act
        await configurationService.LoadConfigAsync(() => new TestConfig(), CancellationToken);
        configurationService.Dispose();

        // Assert
        events.ShouldBe([nameof(ConfigService<>.IsLoaded), nameof(ConfigService<>.Config)]);
    }

    [Fact]
    public async Task UpdateConfigAsync_WritesConfigAndUpdatesCachedConfig()
    {
        // Arrange
        var assetsService = new MemoryAssetsService(BasePath);
        var configurationService = new ConfigService<TestConfig>(ConfigFileName, assetsService);
        var newConfig = new TestConfig { Setting = "New" };

        // Act
        await configurationService.LoadConfigAsync(() => new TestConfig(), CancellationToken);

        // Act
        TestConfig returnedConfig = await configurationService.UpdateConfigAsync(_ => newConfig, CancellationToken);
        configurationService.Dispose();

        // Assert
        returnedConfig.ShouldBeEquivalentTo(newConfig);
        configurationService.IsLoaded.ShouldBeTrue();
        configurationService.Config.ShouldBeEquivalentTo(newConfig);
        TestConfig writtenConfig = await assetsService.DeserializeJsonAsync<TestConfig>(
            ConfigFileName,
            cancellationToken: CancellationToken
        );
        writtenConfig.ShouldBe(newConfig);
    }

    [Fact]
    public async Task UpdateConfigAsync_WhenNotLoaded_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var assetsService = new MemoryAssetsService(BasePath);
        var configurationService = new ConfigService<TestConfig>(ConfigFileName, assetsService);

        // Act
        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await configurationService.UpdateConfigAsync(config => config, CancellationToken)
        );
        configurationService.Dispose();

        // Assert
        assetsService.Exists(ConfigFileName).ShouldBeFalse();
    }

    [Fact]
    public async Task UpdateConfigAsync_WhenUpdateFuncReturnsNull_ShouldThrowAndKeepExistingConfig()
    {
        // Arrange
        var assetsService = new MemoryAssetsService(BasePath);
        var configurationService = new ConfigService<TestConfig>(ConfigFileName, assetsService);
        var initialConfig = new TestConfig { Setting = "Initial" };
        await configurationService.LoadConfigAsync(() => initialConfig, CancellationToken);

        // Act
        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await configurationService.UpdateConfigAsync(_ => null!, CancellationToken)
        );
        configurationService.Dispose();

        // Assert
        configurationService.Config.ShouldBe(initialConfig);
        TestConfig writtenConfig = await assetsService.DeserializeJsonAsync<TestConfig>(
            ConfigFileName,
            cancellationToken: CancellationToken
        );
        writtenConfig.ShouldBe(initialConfig);
    }

    [Fact]
    public void Config_PropertyNotLoaded_ThrowsInvalidOperationException()
    {
        // Act
        var assetsService = new MemoryAssetsService(BasePath);
        var configurationService = new ConfigService<TestConfig>(ConfigFileName, assetsService);
        configurationService.Dispose();

        // Assert
        configurationService.IsLoaded.ShouldBeFalse();
        Should.Throw<InvalidOperationException>(() => configurationService.Config);
    }

    [Fact]
    public void Config_Path_ReturnsCorrectPath()
    {
        // Arrange
        var assetsService = new MemoryAssetsService(BasePath);
        var configurationService = new ConfigService<TestConfig>(ConfigFileName, assetsService);

        // Assert
        configurationService.Path.ShouldBe(Path.Join(BasePath, ConfigFileName));
        configurationService.Dispose();
    }

    [Fact]
    public async Task Config_PropertyChanged_ShouldNotRaiseOnSameUpdate()
    {
        // Arrange
        var assetsService = new MemoryAssetsService(BasePath);
        var configurationService = new ConfigService<TestConfig>(ConfigFileName, assetsService);
        List<string?> _events = [];
        configurationService.PropertyChanged += (_, args) => _events.Add(args.PropertyName);

        // Act
        var initialConfig = new TestConfig();
        await configurationService.LoadConfigAsync(() => initialConfig, CancellationToken);
        _events.Clear();

        // Act
        await configurationService.UpdateConfigAsync(config => config, CancellationToken);
        configurationService.Dispose();

        // Assert
        _events.ShouldBeEmpty();
    }

    [Fact]
    public async Task Config_PropertyChanged_ShouldRaiseOnDifferentUpdate()
    {
        // Arrange
        var newConfig = new TestConfig { Setting = "newValue" };
        var assetsService = new MemoryAssetsService(BasePath);
        var configurationService = new ConfigService<TestConfig>(ConfigFileName, assetsService);
        List<string?> _events = [];
        configurationService.PropertyChanged += (_, args) => _events.Add(args.PropertyName);

        // Act
        await configurationService.LoadConfigAsync(() => new TestConfig(), CancellationToken);
        _events.Clear();

        // Act
        await configurationService.UpdateConfigAsync(_ => newConfig, CancellationToken);
        configurationService.Dispose();

        // Assert
        _events.ShouldHaveSingleItem().ShouldBe(nameof(ConfigService<>.Config));
    }

    [Fact]
    public void Config_WhenUsingDI_ShouldWork()
    {
        // Arrange
        ServiceProvider provider = new ServiceCollection()
            .AddMemoryAssetsService(BasePath)
            .AddConfigurationFile<TestConfig>("config.json")
            .BuildServiceProvider();

        ConfigService<TestConfig> service = provider.GetRequiredService<ConfigService<TestConfig>>();
        service.IsLoaded.ShouldBeFalse();
    }

    [Fact]
    public void Config_WhenUsingDI_ShouldWorkWithContext()
    {
        // Arrange
        ServiceProvider provider = new ServiceCollection()
            .AddMemoryAssetsService(BasePath)
            .AddConfigurationFile<TestConfig>("config.json", TestConfigContext.Default.TestConfig)
            .BuildServiceProvider();

        ConfigService<TestConfig> service = provider.GetRequiredService<ConfigService<TestConfig>>();
        service.IsLoaded.ShouldBeFalse();
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

        ConfigService<TestConfig> service = provider.GetRequiredService<ConfigService<TestConfig>>();
        service.IsLoaded.ShouldBeFalse();
        service.Path.ShouldBe(Path.Join("some/other/path", ConfigFileName));
    }

    [Fact]
    public async Task Observe_WhenFirstChangeIsEnumDefault_ShouldEmitChangedValue()
    {
        // Arrange
        var assetsService = new MemoryAssetsService(BasePath);
        var configurationService = new ConfigService<TestConfig>(ConfigFileName, assetsService);
        await configurationService.LoadConfigAsync(
            () => new TestConfig { LogLevel = TestLogLevel.Warning },
            CancellationToken
        );
        var observer = new TestObserver<TestLogLevel>();

        // Act
        using IDisposable subscription = configurationService.Observe(x => x.LogLevel).Subscribe(observer);
        await configurationService.UpdateConfigAsync(
            _ => new TestConfig { LogLevel = TestLogLevel.Verbose },
            CancellationToken
        );

        // Assert
        observer.Values.ShouldHaveSingleItem().ShouldBe(TestLogLevel.Verbose);
        configurationService.Dispose();
    }

    [Fact]
    public async Task Observe_WhenMultipleSubscribersOnSameObservable_ShouldEmitToEachSubscriber()
    {
        // Arrange
        var assetsService = new MemoryAssetsService(BasePath);
        var configurationService = new ConfigService<TestConfig>(ConfigFileName, assetsService);
        await configurationService.LoadConfigAsync(
            () => new TestConfig { LogLevel = TestLogLevel.Warning },
            CancellationToken
        );
        IObservable<TestLogLevel> observable = configurationService.Observe(x => x.LogLevel);
        var observer1 = new TestObserver<TestLogLevel>();
        var observer2 = new TestObserver<TestLogLevel>();

        // Act
        using IDisposable subscription1 = observable.Subscribe(observer1);
        using IDisposable subscription2 = observable.Subscribe(observer2);
        await configurationService.UpdateConfigAsync(
            _ => new TestConfig { LogLevel = TestLogLevel.Verbose },
            CancellationToken
        );

        // Assert
        observer1.Values.ShouldHaveSingleItem().ShouldBe(TestLogLevel.Verbose);
        observer2.Values.ShouldHaveSingleItem().ShouldBe(TestLogLevel.Verbose);
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
