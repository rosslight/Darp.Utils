namespace Darp.Utils.Configuration;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization.Metadata;
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
    /// <param name="typeInfo">Metadata about the type to convert.</param>
    /// <typeparam name="TConfig"> The type of the config </typeparam>
    /// <typeparam name="TAssetsService"> The type of the assets service </typeparam>
    /// <returns> A reference to this instance after the operation has completed. </returns>
    public static IServiceCollection AddConfigurationFile<TConfig, TAssetsService>(
        this IServiceCollection serviceCollection,
        string configFileName,
        JsonTypeInfo<TConfig> typeInfo)
        where TAssetsService : IAssetsService where TConfig : new() => serviceCollection
        .AddSingleton<IConfigurationService<TConfig>>(provider => new ConfigurationService<TConfig>(configFileName, provider.GetRequiredService<TAssetsService>(), typeInfo));

    /// <summary>
    /// Adds a new configuration file and registers a corresponding <see cref="IConfigurationService{TConfig}"/>.
    /// Requires a configured <see cref="IAssetsService"/> of type <typeparamref name="TAssetsService"/>
    /// </summary>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="configFileName"> The name of the config file containing the <typeparamref name="TConfig"/> </param>
    /// <typeparam name="TConfig"> The type of the config </typeparam>
    /// <typeparam name="TAssetsService"> The type of the assets service </typeparam>
    /// <returns> A reference to this instance after the operation has completed. </returns>
    [RequiresUnreferencedCode("JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved.")]
    [RequiresDynamicCode("JSON serialization and deserialization might require types that cannot be statically analyzed and might need runtime code generation. Use System.Text.Json source generation for native AOT applications.")]
    public static IServiceCollection AddConfigurationFile<TConfig, TAssetsService>(
        this IServiceCollection serviceCollection,
        string configFileName)
        where TAssetsService : IAssetsService where TConfig : new() => serviceCollection
        .AddSingleton<IConfigurationService<TConfig>>(provider => new ConfigurationService<TConfig>(configFileName, provider.GetRequiredService<TAssetsService>()));
}
