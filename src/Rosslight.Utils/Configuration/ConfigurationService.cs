using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Rosslight.Utils.Assets;

namespace Rosslight.Utils.Configuration;

/// <inheritdoc />
public sealed class ConfigurationService<TConfig> : IConfigurationService<TConfig>
    where TConfig : new()
{
    private readonly string _configFileName;
    //private readonly IReadOnlyAssetsService _defaultAssetsService;
    private readonly IAssetsService _configurationAssetsService;
    private TConfig _config = new();
    private bool _isLoaded;
    private readonly SemaphoreSlim _semaphore = new(1);

    /// <summary> Instantiate a new Configuration service </summary>
    /// <param name="configFileName">The name of the config file</param>
    /// <param name="configurationAssetsService">The assets service to be read from or written to</param>
    public ConfigurationService(string configFileName, IAssetsService configurationAssetsService)
    {
        _configFileName = configFileName;
        //_defaultAssetsService = defaultAssetsService;
        _configurationAssetsService = configurationAssetsService;
    }

    /// <summary> The JsonSerializerOptions used </summary>
    public JsonSerializerOptions WriteOptions { get; } = new() { WriteIndented = true };
    /// <inheritdoc />
    public bool IsLoaded { get => _isLoaded; private set => SetField(ref _isLoaded, value); }

    /// <inheritdoc />
    public TConfig Config { get => _config; private set => SetField(ref _config, value); }

    /// <inheritdoc />
    public string Path => System.IO.Path.Combine(_configurationAssetsService.BasePath, _configFileName);

    /// <inheritdoc />
    public async Task<TConfig> LoadConfigurationAsync(CancellationToken cancellationToken)
    {
        if (!_configurationAssetsService.Exists(_configFileName))
        {
            IsLoaded = true;
            return Config;
        }

        await using Stream stream = _configurationAssetsService.GetReadOnlySteam(_configFileName);
        Config = await JsonSerializer.DeserializeAsync<TConfig>(stream, cancellationToken: cancellationToken)
                 ?? throw new Exception("Deserialization yielded no result");
        IsLoaded = true;
        return Config;
    }

    /// <inheritdoc />
    public async Task<TConfig> WriteConfigurationAsync(TConfig configuration, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        await _configurationAssetsService.SerializeJsonAsync(_configFileName, configuration, WriteOptions, cancellationToken);
        Config = configuration;
        IsLoaded = true;
        _semaphore.Release();
        return Config;
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}