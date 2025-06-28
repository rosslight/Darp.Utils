namespace Darp.Utils.Configuration;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization.Metadata;
using Assets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary> Extension class for <see cref="IServiceCollection"/> </summary>
public static class ConfigurationServiceCollectionExtensions
{
    /// <summary>
    /// Adds a new configuration file and registers a corresponding <see cref="IConfigurationService{TConfig}"/>.
    /// Requires a configured <see cref="IAssetsService"/> with the given <paramref name="assetsName"/>
    /// </summary>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="assetsName"> The name of the assets service </param>
    /// <param name="configFileName"> The name of the config file containing the <typeparamref name="TConfig"/> </param>
    /// <param name="typeInfo">Metadata about the type to convert.</param>
    /// <typeparam name="TConfig"> The type of the config </typeparam>
    /// <returns> A reference to this instance after the operation has completed. </returns>
    /// <remarks> Will only be added if the configuration service hasn't already been registered. </remarks>
    public static IServiceCollection AddConfigurationFile<TConfig>(
        this IServiceCollection serviceCollection,
        string? assetsName,
        string configFileName,
        JsonTypeInfo<TConfig> typeInfo
    )
        where TConfig : new()
    {
        serviceCollection.TryAddSingleton<IConfigurationService<TConfig>>(provider => new ConfigurationService<TConfig>(
            configFileName,
            provider.GetRequiredService<IAssetsFactory>().GetAssets(assetsName),
            typeInfo
        ));
        return serviceCollection;
    }

    /// <summary>
    /// Adds a new configuration file and registers a corresponding <see cref="IConfigurationService{TConfig}"/>.
    /// Requires a configured <see cref="IAssetsService"/>
    /// </summary>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="configFileName"> The name of the config file containing the <typeparamref name="TConfig"/> </param>
    /// <param name="typeInfo">Metadata about the type to convert.</param>
    /// <typeparam name="TConfig"> The type of the config </typeparam>
    /// <returns> A reference to this instance after the operation has completed. </returns>
    /// <remarks> Will only be added if the configuration service hasn't already been registered. </remarks>
    public static IServiceCollection AddConfigurationFile<TConfig>(
        this IServiceCollection serviceCollection,
        string configFileName,
        JsonTypeInfo<TConfig> typeInfo
    )
        where TConfig : new() => serviceCollection.AddConfigurationFile(null, configFileName, typeInfo);

    /// <summary>
    /// Adds a new configuration file and registers a corresponding <see cref="IConfigurationService{TConfig}"/>.
    /// Requires a configured <see cref="IAssetsService"/> with the given <paramref name="assetsName"/>
    /// </summary>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="assetsName"> The name of the assets service </param>
    /// <param name="configFileName"> The name of the config file containing the <typeparamref name="TConfig"/> </param>
    /// <typeparam name="TConfig"> The type of the config </typeparam>
    /// <returns> A reference to this instance after the operation has completed. </returns>
    /// <remarks> Will only be added if the configuration service hasn't already been registered. </remarks>
    [RequiresUnreferencedCode(
        "JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved."
    )]
    [RequiresDynamicCode(
        "JSON serialization and deserialization might require types that cannot be statically analyzed and might need runtime code generation. Use System.Text.Json source generation for native AOT applications."
    )]
    public static IServiceCollection AddConfigurationFile<TConfig>(
        this IServiceCollection serviceCollection,
        string? assetsName,
        string configFileName
    )
        where TConfig : new()
    {
        serviceCollection.TryAddSingleton<IConfigurationService<TConfig>>(provider => new ConfigurationService<TConfig>(
            configFileName,
            provider.GetRequiredService<IAssetsFactory>().GetAssets(assetsName)
        ));
        return serviceCollection;
    }

    /// <summary>
    /// Adds a new configuration file and registers a corresponding <see cref="IConfigurationService{TConfig}"/>.
    /// Requires a configured <see cref="IAssetsService"/>
    /// </summary>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="configFileName"> The name of the config file containing the <typeparamref name="TConfig"/> </param>
    /// <typeparam name="TConfig"> The type of the config </typeparam>
    /// <returns> A reference to this instance after the operation has completed. </returns>
    /// <remarks> Will only be added if the configuration service hasn't already been registered. </remarks>
    [RequiresUnreferencedCode(
        "JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved."
    )]
    [RequiresDynamicCode(
        "JSON serialization and deserialization might require types that cannot be statically analyzed and might need runtime code generation. Use System.Text.Json source generation for native AOT applications."
    )]
    public static IServiceCollection AddConfigurationFile<TConfig>(
        this IServiceCollection serviceCollection,
        string configFileName
    )
        where TConfig : new() => serviceCollection.AddConfigurationFile<TConfig>(null, configFileName);
}
