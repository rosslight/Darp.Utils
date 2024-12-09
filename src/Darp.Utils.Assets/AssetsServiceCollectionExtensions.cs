namespace Darp.Utils.Assets;

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

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
    public static IServiceCollection AddFolderAssetsService(
        this IServiceCollection serviceCollection,
        string folderPath,
        string relativePath
    ) => serviceCollection.AddTransient<IFolderAssetsService>(_ => new FolderAssetsService(folderPath, relativePath));

    /// <summary>
    /// Adds an <see cref="IAppDataAssetsService"/> to the serviceCollection.
    /// Root is the <see cref="Environment.SpecialFolder.ApplicationData"/>
    /// </summary>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="relativePath"> The relative path inside the <see cref="Environment.SpecialFolder.ApplicationData"/> </param>
    /// <returns> A reference to this instance after the operation has completed. </returns>
    public static IServiceCollection AddAppDataAssetsService(
        this IServiceCollection serviceCollection,
        string relativePath
    ) => serviceCollection.AddTransient<IAppDataAssetsService>(_ => new AppDataAssetsService(relativePath));

    /// <summary>
    /// Adds an <see cref="IProgramDataAssetsService"/> to the serviceCollection.
    /// Root is the <see cref=" Environment.SpecialFolder.CommonApplicationData"/>
    /// </summary>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="relativePath"> The relative path inside the <see cref="Environment.SpecialFolder.CommonApplicationData"/> </param>
    /// <returns> A reference to this instance after the operation has completed. </returns>
    public static IServiceCollection AddProgramDataAssetsService(
        this IServiceCollection serviceCollection,
        string relativePath
    ) => serviceCollection.AddTransient<IProgramDataAssetsService>(_ => new ProgramDataAssetsService(relativePath));

    /// <summary> Adds an <see cref="IEmbeddedResourceAssetsService"/> to the serviceCollection. </summary>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <typeparamref name="TAssemblyMarker"> A type which is contained in the assembly and can be used as a marker </typeparamref>
    /// <returns> A reference to this instance after the operation has completed. </returns>
    public static IServiceCollection AddEmbeddedResourceAssetsService<TAssemblyMarker>(
        this IServiceCollection serviceCollection
    ) => serviceCollection.AddEmbeddedResourceAssetsService(typeof(TAssemblyMarker).Assembly);

    /// <summary> Adds an <see cref="IEmbeddedResourceAssetsService"/> to the serviceCollection. </summary>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="assembly"> The assembly which contains the EmbeddedResources </param>
    /// <returns> A reference to this instance after the operation has completed. </returns>
    public static IServiceCollection AddEmbeddedResourceAssetsService(
        this IServiceCollection serviceCollection,
        Assembly assembly
    ) =>
        serviceCollection.AddTransient<IEmbeddedResourceAssetsService>(_ => new EmbeddedResourceAssetsService(
            assembly
        ));

    /// <summary> Adds an <see cref="IBaseDirectoryAssetsService"/> to the serviceCollection. </summary>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <returns> A reference to this instance after the operation has completed. </returns>
    public static IServiceCollection AddBaseDirectoryAssetsService(this IServiceCollection serviceCollection) =>
        serviceCollection.AddTransient<IBaseDirectoryAssetsService>(_ => new BaseDirectoryAssetsService());
}
