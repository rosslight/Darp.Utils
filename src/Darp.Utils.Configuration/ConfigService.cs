namespace Darp.Utils.Configuration;

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Assets;

/// <summary> Instantiate a new Configuration service </summary>
/// <param name="configFileName">The name of the config file</param>
/// <param name="configurationAssetsService">The asset service to be read from or written to</param>
/// <param name="typeInfo">Metadata about the type to convert.</param>
public sealed class ConfigService<TConfig>(
    string configFileName,
    IAssetsService configurationAssetsService,
    JsonTypeInfo<TConfig> typeInfo
) : IDisposable, INotifyPropertyChanged
    where TConfig : notnull
{
    private const string JsonTypeResolverUnreferencedCode =
        "JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved.";
    private const string JsonTypeResolverDynamicCode =
        "JSON serialization and deserialization might require types that cannot be statically analyzed and might need runtime code generation. Use System.Text.Json source generation for native AOT applications.";

    // A semaphore that protects the write config operation
    private readonly SemaphoreSlim _readWriteSemaphore = new(1);
    private readonly string _configFileName = configFileName;
    private readonly IAssetsService _configurationAssetsService = configurationAssetsService;
    private readonly JsonTypeInfo<TConfig> _configTypeInfo = typeInfo;
    private ConfigValue _configValue;

    /// <summary> Instantiate a new Configuration service </summary>
    /// <param name="configFileName">The name of the config file</param>
    /// <param name="configurationAssetsService">The assets service to be read from or written to</param>
    [RequiresUnreferencedCode(JsonTypeResolverUnreferencedCode)]
    [RequiresDynamicCode(JsonTypeResolverDynamicCode)]
    public ConfigService(string configFileName, IAssetsService configurationAssetsService)
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

    /// <summary> If true, the config service is disposed of and no longer usable. Methods will throw. </summary>
    public bool IsDisposed { get; private set; }

    /// <summary> Gives back whether a <see cref="Config"/> value is available </summary>
    public bool IsLoaded => _configValue.IsLoaded;

    /// <summary> The current config </summary>
    /// <exception cref="InvalidOperationException">Thrown if the config is not loaded yet</exception>
    public TConfig Config => _configValue.GetValueOrThrow();

    /// <summary> The path to the main config file </summary>
    public string Path => System.IO.Path.Join(_configurationAssetsService.BasePath, _configFileName);

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary> (Re)load the configuration. Updates the <see cref="Config"/> property </summary>
    /// <param name="initialConfigProvider">A callback to be called when the config could not be found and has to be created</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>The new config</returns>
    /// <remarks>This method may not be called from a change event caused by either <see cref="LoadConfigAsync"/> or <see cref="UpdateConfigAsync"/></remarks>
    public async Task<TConfig> LoadConfigAsync(
        Func<TConfig> initialConfigProvider,
        CancellationToken cancellationToken = default
    )
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        ArgumentNullException.ThrowIfNull(initialConfigProvider);

        await _readWriteSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            TConfig newConfig;
            if (!_configurationAssetsService.Exists(_configFileName))
            {
                newConfig = initialConfigProvider();
                ThrowIfNull(newConfig);
                await _configurationAssetsService
                    .SerializeJsonAsync(_configFileName, newConfig, _configTypeInfo, cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                newConfig = await _configurationAssetsService
                    .DeserializeJsonAsync(_configFileName, _configTypeInfo, cancellationToken)
                    .ConfigureAwait(false);
            }
            SetConfig(newConfig, out var configChanged, out var hasIsLoadedChanged);
            RaiseIfPropertyChanged(hasIsLoadedChanged, nameof(IsLoaded));
            RaiseIfPropertyChanged(configChanged, nameof(Config));
            return newConfig;
        }
        finally
        {
            _readWriteSemaphore.Release();
        }
    }

    /// <summary> Updates the <see cref="Config"/>. </summary>
    /// <param name="updateFunc">
    /// The update function. Provides the current value of the config to reduce races.
    /// DO NOT EDIT the current value of the config. Changes will not be applied
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>The new config</returns>
    /// <remarks>This method may not be called from a change event caused by either <see cref="LoadConfigAsync"/> or <see cref="UpdateConfigAsync"/></remarks>
    public async Task<TConfig> UpdateConfigAsync(
        Func<TConfig, TConfig> updateFunc,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(updateFunc);
        ObjectDisposedException.ThrowIf(IsDisposed, this);

        await _readWriteSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!IsLoaded)
                throw new InvalidOperationException("Config is not loaded yet!");
            TConfig newConfig = updateFunc(Config);
            ThrowIfNull(newConfig);
            if (EqualityComparer<TConfig>.Default.Equals(_configValue.Value, newConfig))
                return _configValue.GetValueOrThrow();
            await _configurationAssetsService
                .SerializeJsonAsync(_configFileName, newConfig, _configTypeInfo, cancellationToken)
                .ConfigureAwait(false);
            SetConfig(newConfig, out var configChanged, out var hasIsLoadedChanged);
            RaiseIfPropertyChanged(hasIsLoadedChanged, nameof(IsLoaded));
            RaiseIfPropertyChanged(configChanged, nameof(Config));
            return newConfig;
        }
        finally
        {
            _readWriteSemaphore.Release();
        }
    }

    private void SetConfig(TConfig value, out bool hasConfigChanged, out bool hasIsLoadedChanged)
    {
        ThrowIfNull(value);
        ConfigValue oldConfig = _configValue;
        _configValue = new ConfigValue(true, value);
        hasConfigChanged =
            !oldConfig.IsLoaded || !EqualityComparer<TConfig>.Default.Equals(oldConfig.Value, _configValue.Value);
        hasIsLoadedChanged = oldConfig.IsLoaded != _configValue.IsLoaded;
    }

    private static void ThrowIfNull([NotNull] TConfig? value)
    {
        if (value is null)
            throw new InvalidOperationException("Config cannot be null.");
    }

    private void RaiseIfPropertyChanged(bool isChanged, [CallerMemberName] string? propertyName = null)
    {
        if (!isChanged)
            return;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        IsDisposed = true;
        _readWriteSemaphore.Dispose();
    }

    private readonly record struct ConfigValue
    {
        public ConfigValue(bool isLoaded, TConfig value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            IsLoaded = isLoaded;
            Value = value;
        }

        [MemberNotNullWhen(true, nameof(Value))]
        public bool IsLoaded { get; }
        public TConfig? Value { get; }

        public TConfig GetValueOrThrow() =>
            IsLoaded ? Value : throw new InvalidOperationException("Config is not loaded yet!");
    }
}
