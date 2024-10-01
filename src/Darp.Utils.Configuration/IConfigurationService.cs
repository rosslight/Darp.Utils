namespace Darp.Utils.Configuration;

using System.ComponentModel;

/// <summary> A generic interface for a configuration file </summary>
/// <typeparam name="TConfig">The type describing the underlying config</typeparam>
public interface IConfigurationService<TConfig> : IDisposable, INotifyPropertyChanged
{
    /// <summary> Gives back whether a config value is available </summary>
    bool IsLoaded { get; }

    /// <summary> Gives back whether the config service is disposed </summary>
    bool IsDisposed { get; }

    /// <summary> The config </summary>
    TConfig Config { get; }

    /// <summary> The path to the main config file </summary>
    string Path { get; }

    /// <summary> (Re)load the configuration. Updates the <see cref="Config"/> property </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>The new config</returns>
    Task<TConfig> LoadConfigurationAsync(CancellationToken cancellationToken = default);

    /// <summary> Saves a new config. Updates the <see cref="Config"/> property. </summary>
    /// <param name="configuration">The configuration to be saved</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>The new config</returns>
    Task<TConfig> WriteConfigurationAsync(
        TConfig configuration,
        CancellationToken cancellationToken = default
    );
}
