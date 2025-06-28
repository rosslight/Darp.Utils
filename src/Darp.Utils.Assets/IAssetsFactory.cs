namespace Darp.Utils.Assets;

/// <summary> Factory for creating assets services. </summary>
public interface IAssetsFactory
{
    /// <summary> Get the default assets service. </summary>
    /// <returns> The default assets service. </returns>
    IReadOnlyAssetsService GetReadOnlyAssets() => GetReadOnlyAssets<IReadOnlyAssetsService>();

    /// <summary> Get the assets service with the given name. </summary>
    /// <param name="name"> The name of the assets service. </param>
    /// <returns> The assets service with the given name. </returns>
    IReadOnlyAssetsService GetReadOnlyAssets(string name) => GetReadOnlyAssets<IReadOnlyAssetsService>(name);

    /// <summary> Get the writable assets service. </summary>
    /// <returns> The writable assets service. </returns>
    IAssetsService GetAssets() => GetReadOnlyAssets<IAssetsService>();

    /// <summary> Get the writable assets service with the given name. </summary>
    /// <param name="name"> The name of the assets service. </param>
    /// <returns> The writable assets service with the given name. </returns>
    IAssetsService GetAssets(string? name) => GetReadOnlyAssets<IAssetsService>(name);

    /// <summary> Get the assets service with the given type. </summary>
    /// <typeparam name="TAssetsService"> The type of the assets service. </typeparam>
    /// <returns> The assets service with the given type. </returns>
    TAssetsService GetReadOnlyAssets<TAssetsService>()
        where TAssetsService : IReadOnlyAssetsService;

    /// <summary> Get the assets service with the given type and name. </summary>
    /// <typeparam name="TAssetsService"> The type of the assets service. </typeparam>
    /// <param name="name"> The name of the assets service. </param>
    /// <returns> The assets service with the given type and name. </returns>
    TAssetsService GetReadOnlyAssets<TAssetsService>(string? name)
        where TAssetsService : IReadOnlyAssetsService;
}
