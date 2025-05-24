namespace Darp.Utils.Configuration;

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Assets;
using Assets.Abstractions;

/// <inheritdoc cref="IConfigurationService{TConfig}"/>
/// <summary> Instantiate a new Configuration service </summary>
/// <param name="configFileName">The name of the config file</param>
/// <param name="configurationAssetsService">The assets service to be read from or written to</param>
/// <param name="typeInfo">Metadata about the type to convert.</param>
public sealed class ConfigurationService<TConfig>(
    string configFileName,
    IAssetsService configurationAssetsService,
    JsonTypeInfo<TConfig> typeInfo
) : IConfigurationService<TConfig>
    where TConfig : new()
{
    private const string JsonTypeResolverUnreferencedCode =
        "JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved.";
    private const string JsonTypeResolverDynamicCode =
        "JSON serialization and deserialization might require types that cannot be statically analyzed and might need runtime code generation. Use System.Text.Json source generation for native AOT applications.";

    private readonly string _configFileName = configFileName;
    private readonly IAssetsService _configurationAssetsService = configurationAssetsService;
    private readonly JsonTypeInfo<TConfig> _configTypeInfo = typeInfo;
    private readonly SemaphoreSlim _semaphore = new(1);

    /// <summary> Instantiate a new Configuration service </summary>
    /// <param name="configFileName">The name of the config file</param>
    /// <param name="configurationAssetsService">The assets service to be read from or written to</param>
    [RequiresUnreferencedCode(JsonTypeResolverUnreferencedCode)]
    [RequiresDynamicCode(JsonTypeResolverDynamicCode)]
    public ConfigurationService(string configFileName, IAssetsService configurationAssetsService)
        : this(configFileName, configurationAssetsService, CreateDefaultJsonTypeInfo()) { }

    [RequiresUnreferencedCode(JsonTypeResolverUnreferencedCode)]
    [RequiresDynamicCode(JsonTypeResolverDynamicCode)]
    private static JsonTypeInfo<TConfig> CreateDefaultJsonTypeInfo()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        };
        return (JsonTypeInfo<TConfig>)options.GetTypeInfo(typeof(TConfig));
    }

    /// <summary> The JsonSerializerOptions used </summary>
    public JsonSerializerOptions WriteOptions => _configTypeInfo.Options;

    /// <inheritdoc />
    public bool IsLoaded
    {
        get;
        private set => SetField(ref field, value);
    }

    /// <inheritdoc />
    public bool IsDisposed
    {
        get;
        private set => SetField(ref field, value);
    }

    /// <inheritdoc />
    public TConfig Config
    {
        get;
        private set => SetField(ref field, value);
    } = new();

    /// <inheritdoc />
    public string Path => System.IO.Path.Join(_configurationAssetsService.BasePath, _configFileName);

    /// <inheritdoc />
    public async Task<TConfig> LoadConfigurationAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        if (!_configurationAssetsService.Exists(_configFileName))
        {
            IsLoaded = true;
            return Config;
        }

        Stream stream = _configurationAssetsService.GetReadOnlyStream(_configFileName);
        await using (stream.ConfigureAwait(false))
        {
            Config =
                await JsonSerializer
                    .DeserializeAsync(stream, _configTypeInfo, cancellationToken: cancellationToken)
                    .ConfigureAwait(false) ?? throw new JsonException("Deserialization yielded no result");
            IsLoaded = true;
            return Config;
        }
    }

    /// <inheritdoc />
    public async Task<TConfig> WriteConfigurationAsync(TConfig configuration, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        await _configurationAssetsService
            .SerializeJsonAsync(_configFileName, configuration, _configTypeInfo, cancellationToken)
            .ConfigureAwait(false);
        Config = configuration;
        IsLoaded = true;
        _semaphore.Release();
        return Config;
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;
        field = value;
        OnPropertyChanged(propertyName);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _semaphore.Dispose();
        IsDisposed = true;
    }
}
