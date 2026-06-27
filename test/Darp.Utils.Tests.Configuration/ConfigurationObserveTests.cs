namespace Darp.Utils.Tests.Configuration;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Assets;
using Darp.Utils.Configuration;
using Shouldly;
using Xunit;

public sealed class ConfigurationObserveTests
{
    private const string ConfigFileName = "testConfig.json";
    private const string BasePath = "some/path";

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Observe_WhenSubscribedBeforeLoad_ShouldEmitFirstValueOnLoadAndThenChangedValues()
    {
        // Arrange
        var assetsService = new MemoryAssetsService(BasePath);
        var configurationService = new ConfigService<TestConfig>(ConfigFileName, assetsService);
        var observer = new TestObserver<TestLogLevel>();

        // Act
        using IDisposable subscription = configurationService.Observe(x => x.LogLevel).Subscribe(observer);
        observer.Values.ShouldBeEmpty();

        await configurationService.LoadConfigAsync(
            () => new TestConfig { LogLevel = TestLogLevel.Warning },
            CancellationToken
        );
        await configurationService.UpdateConfigAsync(
            config => config with { Setting = "Changed setting" },
            CancellationToken
        );
        await configurationService.UpdateConfigAsync(
            config => config with { LogLevel = TestLogLevel.Verbose },
            CancellationToken
        );

        // Assert
        observer.Values.ShouldBe([TestLogLevel.Warning, TestLogLevel.Verbose]);
        configurationService.Dispose();
    }

    [Fact]
    public async Task Observe_WhenSubscribedAfterLoad_ShouldEmitCurrentValueImmediatelyAndThenChangedValues()
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
            config => config with { Setting = "Changed setting" },
            CancellationToken
        );
        await configurationService.UpdateConfigAsync(
            config => config with { LogLevel = TestLogLevel.Verbose },
            CancellationToken
        );

        // Assert
        observer.Values.ShouldBe([TestLogLevel.Warning, TestLogLevel.Verbose]);
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
        observer1.Values.ShouldBe([TestLogLevel.Warning, TestLogLevel.Verbose]);
        observer2.Values.ShouldBe([TestLogLevel.Warning, TestLogLevel.Verbose]);
        configurationService.Dispose();
    }
}
