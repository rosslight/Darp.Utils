namespace Darp.Utils.Assets;

using Microsoft.Extensions.DependencyInjection;

/// <summary> Extension class for <see cref="IServiceCollection"/> </summary>
public static class AssetsServiceCollectionExtensions
{
    /// <summary>
    /// Adds an <see cref="IAppDataAssetsService"/> to the serviceCollection.
    /// Root is the <see cref="Environment.SpecialFolder.ApplicationData"/>
    /// </summary>
    /// <param name="serviceCollection"> The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="relativePath"> The relative path inside the AppData </param>
    /// <returns> A reference to this instance after the operation has completed. </returns>
    public static IServiceCollection AddAppDataAssetsService(this IServiceCollection serviceCollection,
        string relativePath) => serviceCollection
        .AddTransient<IAppDataAssetsService>(_ => new AppDataAssetsService(relativePath));
}
