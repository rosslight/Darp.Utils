namespace Darp.Utils.Configuration;

using Assets.Abstractions;
using Microsoft.Extensions.DependencyInjection;

/// <summary> Extension class for <see cref="IServiceCollection"/> </summary>
public static class ConfigurationServiceCollectionExtensions
{
    /// <summary>
    /// Adds a new configuration file and registers a corresponding <see cref="IConfigurationService{TConfig}"/>.
    /// Requires a configured <see cref="IAssetsService"/> of type <typeparamref name="TAssetsService"/>
    /// </summary>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="configFileName"> The name of the config file containing the <typeparamref name="TConfig"/> </param>
    /// <typeparam name="TConfig"> The type of the config </typeparam>
    /// <typeparam name="TAssetsService"> The type of the assets service </typeparam>
    /// <returns> A reference to this instance after the operation has completed. </returns>
    public static IServiceCollection AddConfigurationFile<TConfig, TAssetsService>(
        this IServiceCollection serviceCollection,
        string configFileName)
        where TAssetsService : IAssetsService where TConfig : new() => serviceCollection
        .AddSingleton<IConfigurationService<TConfig>>(provider =>
        {
            return new ConfigurationService<TConfig>(configFileName, provider.GetRequiredService<TAssetsService>());
        });
}
