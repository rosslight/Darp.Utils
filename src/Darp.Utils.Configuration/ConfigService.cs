namespace Darp.Utils.Configuration;

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Assets;

/// <inheritdoc cref="IConfigurationService{TConfig}"/>
/// <summary> Instantiate a new Configuration service </summary>
/// <param name="configFileName">The name of the config file</param>
/// <param name="configurationAssetsService">The asset service to be read from or written to</param>
/// <param name="typeInfo">Metadata about the type to convert.</param>
public sealed class ConfigService<TConfig>(
    string configFileName,
    IAssetsService configurationAssetsService,
    JsonTypeInfo<TConfig> typeInfo
) : IDisposable, INotifyPropertyChanged
{
    private const string JsonTypeResolverUnreferencedCode =
        "JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved.";
    private const string JsonTypeResolverDynamicCode =
        "JSON serialization and deserialization might require types that cannot be statically analyzed and might need runtime code generation. Use System.Text.Json source generation for native AOT applications.";

    private readonly string _configFileName = configFileName;
    private readonly IAssetsService _configurationAssetsService = configurationAssetsService;
    private readonly JsonTypeInfo<TConfig> _configTypeInfo = typeInfo;

    // A semaphore that protects the write config operation
    private readonly SemaphoreSlim _readWriteSemaphore = new(1);

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

    /// <summary> Gives back whether a config value is available </summary>
    public bool IsLoaded
    {
        get;
        private set => SetField(ref field, value);
    }

    /// <summary> If true, the config service is disposed of and no longer usable. Methods will throw. </summary>
    public bool IsDisposed
    {
        get;
        private set => SetField(ref field, value);
    }

    /// <summary> The current config </summary>
    [field: MaybeNull]
    public TConfig Config
    {
        get => IsLoaded && field is not null ? field : throw new InvalidOperationException("Config is not loaded yet!");
        private set => SetField(ref field!, value);
    }

    /// <summary> The path to the main config file </summary>
    public string Path => System.IO.Path.Join(_configurationAssetsService.BasePath, _configFileName);

    /// <summary> (Re)load the configuration. Updates the <see cref="Config"/> property </summary>
    /// <param name="initialConfigProvider">A callback to be called when the config could not be found and has to be created</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>The new config</returns>
    public async Task<TConfig> LoadConfigAsync(Func<TConfig> initialConfigProvider, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        ArgumentNullException.ThrowIfNull(initialConfigProvider);

        await _readWriteSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!_configurationAssetsService.Exists(_configFileName))
            {
                TConfig initialConfig = initialConfigProvider();
                await _configurationAssetsService
                    .SerializeJsonAsync(_configFileName, initialConfig, _configTypeInfo, cancellationToken)
                    .ConfigureAwait(false);
                Config = initialConfig;
                IsLoaded = true;
                return Config;
            }

            Config = await _configurationAssetsService
                .DeserializeJsonAsync(_configFileName, _configTypeInfo, cancellationToken)
                .ConfigureAwait(false);
            IsLoaded = true;
            return Config;
        }
        finally
        {
            _readWriteSemaphore.Release();
        }
    }

    /// <summary> Updates the <see cref="Config"/>. </summary>
    /// <param name="updateFunc">The update function. Provides the current value of the config to reduce races.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>The new config</returns>
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
            TConfig newConfig = updateFunc(Config);
            await _configurationAssetsService
                .SerializeJsonAsync(_configFileName, newConfig, _configTypeInfo, cancellationToken)
                .ConfigureAwait(false);
            Config = newConfig;
            IsLoaded = true;
            return Config;
        }
        finally
        {
            _readWriteSemaphore.Release();
        }
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void SetField<T>(ref T? field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;
        field = value;
        OnPropertyChanged(propertyName);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _readWriteSemaphore.Dispose();
        IsDisposed = true;
    }
}
