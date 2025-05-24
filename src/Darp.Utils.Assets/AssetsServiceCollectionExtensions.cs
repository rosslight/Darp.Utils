namespace Darp.Utils.Assets;

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary> Extension class for <see cref="IServiceCollection"/> </summary>
public static class AssetsServiceCollectionExtensions
{
    /// <summary>
    /// Adds an <see cref="IAppDataAssetsService"/> to the serviceCollection.
    /// Root is the <see cref="Environment.SpecialFolder.ApplicationData"/>
    /// </summary>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="folderPath"> The path to the folder </param>
    /// <param name="relativePath"> The relative path inside the AppData </param>
    /// <returns> A reference to this instance after the operation has completed. </returns>
    /// <remarks> Will only be added if the asset service hasn't already been registered. </remarks>
    public static IServiceCollection AddFolderAssetsService(
        this IServiceCollection serviceCollection,
        string folderPath,
        string relativePath
    )
    {
        serviceCollection.TryAddTransient<IFolderAssetsService>(_ => new FolderAssetsService(folderPath, relativePath));
        return serviceCollection;
    }

    /// <summary>
    /// Adds an <see cref="IAppDataAssetsService"/> to the serviceCollection.
    /// Root is the <see cref="Environment.SpecialFolder.ApplicationData"/>
    /// </summary>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="relativePath"> The relative path inside the <see cref="Environment.SpecialFolder.ApplicationData"/> </param>
    /// <returns> A reference to this instance after the operation has completed. </returns>
    /// <remarks> Will only be added if the asset service hasn't already been registered. </remarks>
    public static IServiceCollection AddAppDataAssetsService(
        this IServiceCollection serviceCollection,
        string relativePath
    )
    {
        serviceCollection.TryAddTransient<IAppDataAssetsService>(_ => new AppDataAssetsService(relativePath));
        return serviceCollection;
    }

    /// <summary>
    /// Adds an <see cref="IProgramDataAssetsService"/> to the serviceCollection.
    /// Root is the <see cref=" Environment.SpecialFolder.CommonApplicationData"/>
    /// </summary>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="relativePath"> The relative path inside the <see cref="Environment.SpecialFolder.CommonApplicationData"/> </param>
    /// <returns> A reference to this instance after the operation has completed. </returns>
    /// <remarks> Will only be added if the asset service hasn't already been registered. </remarks>
    public static IServiceCollection AddProgramDataAssetsService(
        this IServiceCollection serviceCollection,
        string relativePath
    )
    {
        serviceCollection.TryAddTransient<IProgramDataAssetsService>(_ => new ProgramDataAssetsService(relativePath));
        return serviceCollection;
    }

    /// <summary> Adds an <see cref="IEmbeddedResourceAssetsService"/> to the serviceCollection. </summary>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <typeparamref name="TAssemblyMarker"> A type which is contained in the assembly and can be used as a marker </typeparamref>
    /// <returns> A reference to this instance after the operation has completed. </returns>
    /// <remarks> Will only be added if the asset service hasn't already been registered. </remarks>
    public static IServiceCollection AddEmbeddedResourceAssetsService<TAssemblyMarker>(
        this IServiceCollection serviceCollection
    ) => serviceCollection.AddEmbeddedResourceAssetsService(typeof(TAssemblyMarker).Assembly);

    /// <summary> Adds an <see cref="IEmbeddedResourceAssetsService"/> to the serviceCollection. </summary>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="assembly"> The assembly which contains the EmbeddedResources </param>
    /// <returns> A reference to this instance after the operation has completed. </returns>
    /// <remarks> Will only be added if the asset service hasn't already been registered. </remarks>
    public static IServiceCollection AddEmbeddedResourceAssetsService(
        this IServiceCollection serviceCollection,
        Assembly assembly
    )
    {
        serviceCollection.TryAddTransient<IEmbeddedResourceAssetsService>(_ => new EmbeddedResourceAssetsService(
            assembly
        ));
        return serviceCollection;
    }

    /// <summary> Adds an <see cref="IBaseDirectoryAssetsService"/> to the serviceCollection. </summary>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <returns> A reference to this instance after the operation has completed. </returns>
    /// <remarks> Will only be added if the asset service hasn't already been registered. </remarks>
    public static IServiceCollection AddBaseDirectoryAssetsService(this IServiceCollection serviceCollection)
    {
        serviceCollection.TryAddTransient<IBaseDirectoryAssetsService>(_ => new BaseDirectoryAssetsService());
        return serviceCollection;
    }
}
