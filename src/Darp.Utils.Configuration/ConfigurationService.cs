namespace Darp.Utils.Configuration;

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Assets;
using Assets.Abstractions;

/// <inheritdoc cref="IConfigurationService{TConfig}"/>
public sealed class ConfigurationService<TConfig> : IConfigurationService<TConfig>
    where TConfig : new()
{
    private readonly string _configFileName;
    //private readonly IReadOnlyAssetsService _defaultAssetsService;
    private readonly IAssetsService _configurationAssetsService;
    private TConfig _config = new();
    private bool _isLoaded;
    private bool _isDisposed;
    private readonly SemaphoreSlim _semaphore = new(1);
    private readonly JsonTypeInfo<TConfig> _configTypeInfo;

    /// <summary> Instantiate a new Configuration service </summary>
    /// <param name="configFileName">The name of the config file</param>
    /// <param name="configurationAssetsService">The assets service to be read from or written to</param>
    [RequiresUnreferencedCode("JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved.")]
    [RequiresDynamicCode("JSON serialization and deserialization might require types that cannot be statically analyzed and might need runtime code generation. Use System.Text.Json source generation for native AOT applications.")]
    public ConfigurationService(string configFileName, IAssetsService configurationAssetsService)
        : this(configFileName, configurationAssetsService, CreateDefaultJsonTypeInfo())
    {
    }

    [RequiresUnreferencedCode("JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved.")]
    [RequiresDynamicCode("JSON serialization and deserialization might require types that cannot be statically analyzed and might need runtime code generation. Use System.Text.Json source generation for native AOT applications.")]
    private static JsonTypeInfo<TConfig> CreateDefaultJsonTypeInfo()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        };
        return (JsonTypeInfo<TConfig>)options.GetTypeInfo(typeof(TConfig));
    }

    /// <summary> Instantiate a new Configuration service </summary>
    /// <param name="configFileName">The name of the config file</param>
    /// <param name="configurationAssetsService">The assets service to be read from or written to</param>
    /// <param name="typeInfo">Metadata about the type to convert.</param>
    public ConfigurationService(string configFileName,
        IAssetsService configurationAssetsService,
        JsonTypeInfo<TConfig> typeInfo)
    {
        _configFileName = configFileName;
        //_defaultAssetsService = defaultAssetsService;
        _configurationAssetsService = configurationAssetsService;
        _configTypeInfo = typeInfo;
    }

    /// <summary> The JsonSerializerOptions used </summary>
    public JsonSerializerOptions WriteOptions => _configTypeInfo.Options;
    /// <inheritdoc />
    public bool IsLoaded { get => _isLoaded; private set => SetField(ref _isLoaded, value); }

    /// <inheritdoc />
    public bool IsDisposed { get => _isDisposed; private set => SetField(ref _isDisposed, value); }

    /// <inheritdoc />
    public TConfig Config { get => _config; private set => SetField(ref _config, value); }

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

        Stream stream = _configurationAssetsService.GetReadOnlySteam(_configFileName);
        await using (stream.ConfigureAwait(false))
        {
            Config = await JsonSerializer.DeserializeAsync(stream, _configTypeInfo, cancellationToken: cancellationToken)
                         .ConfigureAwait(false)
                     ?? throw new JsonException("Deserialization yielded no result");
            IsLoaded = true;
            return Config;
        }
    }

    /// <inheritdoc />
    public async Task<TConfig> WriteConfigurationAsync(TConfig configuration, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        await _configurationAssetsService.SerializeJsonAsync(_configFileName, configuration, _configTypeInfo, cancellationToken)
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
        {
            return;
        }
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
