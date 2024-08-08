namespace Darp.Utils.Tests.Configuration;

using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Assets;
using Common;
using Darp.Utils.Configuration;
using FluentAssertions;
using NSubstitute;
using Serilog;
using Xunit;
using Xunit.Abstractions;

public sealed class ConfigurationServiceTests(ITestOutputHelper outputHelper)
{
    private const string ConfigFileName = "testConfig.json";
    private readonly ILogger _logger = new LoggerConfiguration()
        .WriteTo
        .TestOutput(outputHelper, formatProvider:CultureInfo.InvariantCulture)
        .CreateLogger();

    private static void CreateServices(out IAssetsService assetsService,
        out ConfigurationService<TestConfig> configurationService)
    {
        assetsService = Substitute.For<IAssetsService>();
        configurationService = new ConfigurationService<TestConfig>(ConfigFileName, assetsService);
    }

    [Fact]
    public async Task LoadConfigurationAsync_FileExists_DeserializesAndReturnsConfig()
    {
        // Arrange
        CreateServices(out IAssetsService assetsService, out ConfigurationService<TestConfig> configurationService);
        var expectedConfig = new TestConfig { Setting = "Test" };
        var json = JsonSerializer.Serialize(expectedConfig, configurationService.WriteOptions);
        using var ms = new MemoryStream();
        await using var writer = new StreamWriter(ms);
        await writer.WriteAsync(json);
        await writer.FlushAsync();
        ms.Position = 0;

        assetsService.Exists(ConfigFileName).Returns(true);
        assetsService.GetReadOnlySteam(ConfigFileName).Returns(ms);

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
        CreateServices(out IAssetsService assetsService, out ConfigurationService<TestConfig> configurationService);
        var expectedConfig = new TestConfig { Setting = "Default" };
        using var ms = new MemoryStream();
        await JsonSerializer.SerializeAsync(ms, expectedConfig, configurationService.WriteOptions);
        ms.Position = 0;
        var buffer = new byte[100];

        assetsService.Exists(ConfigFileName).Returns(false);
        assetsService.GetWriteOnlySteam(ConfigFileName).Returns(new MemoryStream(buffer));
        assetsService.GetReadOnlySteam(ConfigFileName).Returns(_ => new MemoryStream(buffer.TrimBufferEnd()));

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
        CreateServices(out IAssetsService assetsService, out ConfigurationService<TestConfig> configurationService);
        var newConfig = new TestConfig { Setting = "New" };
        var buffer = new byte[100];
        var ms = new MemoryStream(buffer);
        assetsService.GetWriteOnlySteam(ConfigFileName).Returns(ms);

        // Act
        _logger.Information("BufferBeforeWrite: {Bytes}", buffer);
        TestConfig returnedConfig = await configurationService.WriteConfigurationAsync(newConfig, CancellationToken.None);
        configurationService.Dispose();
        _logger.Information("BufferAfterWrite: {Bytes}", buffer);

        // Assert
        returnedConfig.Should().BeEquivalentTo(newConfig);
        configurationService.IsLoaded.Should().BeTrue();
        configurationService.Config.Should().BeEquivalentTo(newConfig);
        TestConfig? writtenConfig = await JsonSerializer.DeserializeAsync<TestConfig>(new MemoryStream(buffer.TrimBufferEnd()));
        writtenConfig.Should().BeEquivalentTo(newConfig);
    }

    [Fact]
    public void Config_PropertyNotLoaded_ThrowsNullReferenceException()
    {
        // Act
        CreateServices(out IAssetsService _, out ConfigurationService<TestConfig> configurationService);
        configurationService.Dispose();

        // Assert
        configurationService.IsLoaded.Should().BeFalse();
        configurationService.Config.Should().BeEquivalentTo(new TestConfig());
    }
}

internal sealed record TestConfig
{
    public string Setting { get; init; } = "Default";
}
