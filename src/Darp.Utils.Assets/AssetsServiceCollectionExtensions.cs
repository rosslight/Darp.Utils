namespace Darp.Utils.Assets;

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary> Extension class for <see cref="IServiceCollection"/> </summary>
public static class AssetsServiceCollectionExtensions
{
    /// <summary>
    /// Adds an <typeparamref name="TAssetsService"/> to the serviceCollection.
    /// The service will be added as a <see cref="IReadOnlyAssetsService"/> and <see cref="IAssetsService"/> if the type implements them.
    /// </summary>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="name"> The name of the specific assets service </param>
    /// <param name="onRequestService"> The function to create the assets service </param>
    /// <param name="assetsServiceLifetime"> The lifetime of the assets service </param>
    /// <typeparam name="TAssetsService"> The type of the assets service </typeparam>
    /// <returns> A reference to this instance after the operation has completed. </returns>
    public static IServiceCollection AddAssetsService<TAssetsService>(
        this IServiceCollection serviceCollection,
        string? name,
        Func<IServiceProvider, TAssetsService> onRequestService,
        ServiceLifetime assetsServiceLifetime = ServiceLifetime.Transient
    )
        where TAssetsService : class, IReadOnlyAssetsService
    {
        ArgumentNullException.ThrowIfNull(serviceCollection);
        serviceCollection.TryAddSingleton<IAssetsFactory, AssetsFactory>();
        var descriptor = new ServiceDescriptor(
            typeof(AssetsOptions),
            provider => new AssetsOptions(name, typeof(TAssetsService), provider, onRequestService),
            assetsServiceLifetime
        );
        serviceCollection.Add(descriptor);
        serviceCollection.AddTransient<TAssetsService>(provider =>
            provider.GetRequiredService<IAssetsFactory>().GetReadOnlyAssets<TAssetsService>(name)
        );
        serviceCollection.AddTransient<IReadOnlyAssetsService>(provider =>
            provider.GetRequiredService<IAssetsFactory>().GetReadOnlyAssets<IReadOnlyAssetsService>(name)
        );
        if (typeof(TAssetsService).IsAssignableTo(typeof(IAssetsService)))
        {
            serviceCollection.AddTransient<IAssetsService>(provider =>
                provider.GetRequiredService<IAssetsFactory>().GetReadOnlyAssets<IAssetsService>(name)
            );
        }
        return serviceCollection;
    }

    /// <summary>
    /// Adds an <see cref="FolderAssetsService"/> to the serviceCollection.
    /// Root is the <paramref name="folderPath"/> with the <paramref name="relativePath"/> appended.
    /// </summary>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="name"> The name of the specific folder assets service </param>
    /// <param name="folderPath"> The path to the folder. </param>
    /// <param name="relativePath"> The relative path inside the <paramref name="folderPath"/> </param>
    /// <returns> A reference to this instance after the operation has completed. </returns>
    public static IServiceCollection AddFolderAssetsService(
        this IServiceCollection serviceCollection,
        string? name,
        string folderPath,
        string relativePath
    )
    {
        return serviceCollection
            .AddAssetsService<FolderAssetsService>(name, _ => new FolderAssetsService(folderPath, relativePath))
            .AddTransient<IFolderAssetsService>(provider => provider.GetRequiredService<FolderAssetsService>());
    }

    /// <summary>
    /// Adds an <see cref="FolderAssetsService"/> to the serviceCollection.
    /// Root is the <paramref name="folderPath"/> with the <paramref name="relativePath"/> appended.
    /// </summary>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="folderPath"> The path to the folder. </param>
    /// <param name="relativePath"> The relative path inside the <paramref name="folderPath"/> </param>
    /// <returns> A reference to this instance after the operation has completed. </returns>
    public static IServiceCollection AddFolderAssetsService(
        this IServiceCollection serviceCollection,
        string folderPath,
        string relativePath
    ) => serviceCollection.AddFolderAssetsService(null, folderPath, relativePath);

    /// <summary>
    /// Adds an <see cref="FolderAssetsService"/> to the serviceCollection.
    /// Root is the <see cref="Environment.SpecialFolder.ApplicationData"/> with the <paramref name="relativePath"/> appended.
    /// </summary>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="relativePath"> The relative path inside the <see cref="Environment.SpecialFolder.ApplicationData"/> </param>
    /// <returns> A reference to this instance after the operation has completed. </returns>
    public static IServiceCollection AddAppDataAssetsService(
        this IServiceCollection serviceCollection,
        string relativePath
    ) => serviceCollection.AddAppDataAssetsService(null, relativePath);

    /// <summary>
    /// Adds an <see cref="FolderAssetsService"/> to the serviceCollection.
    /// Root is the <see cref="Environment.SpecialFolder.ApplicationData"/> with the <paramref name="relativePath"/> appended.
    /// </summary>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="name"> The name of the specific application data assets service </param>
    /// <param name="relativePath"> The relative path inside the <see cref="Environment.SpecialFolder.ApplicationData"/> </param>
    /// <returns> A reference to this instance after the operation has completed. </returns>
    public static IServiceCollection AddAppDataAssetsService(
        this IServiceCollection serviceCollection,
        string? name,
        string relativePath
    )
    {
        return serviceCollection.AddFolderAssetsService(
            name,
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            relativePath
        );
    }

    /// <summary>
    /// Adds an <see cref="FolderAssetsService"/> to the serviceCollection.
    /// Root is the <see cref=" Environment.SpecialFolder.CommonApplicationData"/> with the <paramref name="relativePath"/> appended.
    /// </summary>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="relativePath"> The relative path inside the <see cref="Environment.SpecialFolder.CommonApplicationData"/> </param>
    /// <returns> A reference to this instance after the operation has completed. </returns>
    public static IServiceCollection AddProgramDataAssetsService(
        this IServiceCollection serviceCollection,
        string relativePath
    ) => serviceCollection.AddProgramDataAssetsService(null, relativePath);

    /// <summary>
    /// Adds an <see cref="FolderAssetsService"/> to the serviceCollection.
    /// Root is the <see cref=" Environment.SpecialFolder.CommonApplicationData"/> with the <paramref name="relativePath"/> appended.
    /// </summary>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="name"> The name of the specific embedded resource assets service </param>
    /// <param name="relativePath"> The relative path inside the <see cref="Environment.SpecialFolder.CommonApplicationData"/> </param>
    /// <returns> A reference to this instance after the operation has completed. </returns>
    public static IServiceCollection AddProgramDataAssetsService(
        this IServiceCollection serviceCollection,
        string? name,
        string relativePath
    )
    {
        return serviceCollection.AddFolderAssetsService(
            name,
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            relativePath
        );
    }

    /// <summary> Adds an <see cref="EmbeddedResourceAssetsService"/> to the serviceCollection. </summary>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <typeparamref name="TAssemblyMarker"> A type which is contained in the assembly and can be used as a marker </typeparamref>
    /// <returns> A reference to this instance after the operation has completed. </returns>
    public static IServiceCollection AddEmbeddedResourceAssetsService<TAssemblyMarker>(
        this IServiceCollection serviceCollection
    ) => serviceCollection.AddEmbeddedResourceAssetsService(typeof(TAssemblyMarker).Assembly);

    /// <summary> Adds an <see cref="EmbeddedResourceAssetsService"/> to the serviceCollection. </summary>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="assembly"> The assembly which contains the EmbeddedResources </param>
    /// <returns> A reference to this instance after the operation has completed. </returns>
    public static IServiceCollection AddEmbeddedResourceAssetsService(
        this IServiceCollection serviceCollection,
        Assembly assembly
    ) => serviceCollection.AddEmbeddedResourceAssetsService(null, assembly);

    /// <summary> Adds an <see cref="EmbeddedResourceAssetsService"/> to the serviceCollection. </summary>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="name"> The name of the specific embedded resource assets service </param>
    /// <param name="assembly"> The assembly which contains the EmbeddedResources </param>
    /// <returns> A reference to this instance after the operation has completed. </returns>
    public static IServiceCollection AddEmbeddedResourceAssetsService(
        this IServiceCollection serviceCollection,
        string? name,
        Assembly assembly
    )
    {
        return serviceCollection.AddAssetsService<EmbeddedResourceAssetsService>(
            name,
            _ => new EmbeddedResourceAssetsService(assembly)
        );
    }

    /// <summary>
    /// Adds an <see cref="FolderAssetsService"/> to the serviceCollection.
    /// Root is the <see cref="AppDomain.CurrentDomain"/>.<see cref="AppDomain.BaseDirectory"/>.
    /// </summary>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <returns> A reference to this instance after the operation has completed. </returns>
    public static IServiceCollection AddBaseDirectoryAssetsService(this IServiceCollection serviceCollection) =>
        serviceCollection.AddFolderAssetsService(AppDomain.CurrentDomain.BaseDirectory, string.Empty);

    /// <summary> Adds an <see cref="MemoryAssetsService"/> to the serviceCollection. </summary>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="name"> The name of the specific memory assets service </param>
    /// <param name="basePath"> The base path of the memory assets service </param>
    /// <param name="configure"> The action to configure the memory assets service </param>
    /// <returns> A reference to this instance after the operation has completed. </returns>
    public static IServiceCollection AddMemoryAssetsService(
        this IServiceCollection serviceCollection,
        string? name,
        string basePath,
        Action<IAssetsService>? configure = null
    )
    {
        return serviceCollection.AddAssetsService<MemoryAssetsService>(
            name,
            _ =>
            {
                var service = new MemoryAssetsService(basePath);
                configure?.Invoke(service);
                return service;
            },
            ServiceLifetime.Singleton
        );
    }

    /// <summary> Adds an <see cref="MemoryAssetsService"/> to the serviceCollection. </summary>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="basePath"> The base path of the memory assets service </param>
    /// <param name="configure"> The action to configure the memory assets service </param>
    /// <returns> A reference to this instance after the operation has completed. </returns>
    public static IServiceCollection AddMemoryAssetsService(
        this IServiceCollection serviceCollection,
        string basePath,
        Action<IAssetsService>? configure = null
    ) => serviceCollection.AddMemoryAssetsService(null, basePath, configure);
}

internal sealed record AssetsOptions(
    string? Name,
    Type Type,
    IServiceProvider Services,
    Func<IServiceProvider, IReadOnlyAssetsService> Builder
);
