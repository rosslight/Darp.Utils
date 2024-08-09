namespace Darp.Utils.Assets;

using Darp.Utils.Assets.Abstractions;

/// <summary> An interface for reading from and writing to the <see cref="Environment.SpecialFolder.ApplicationData"/> </summary>
public interface IAppDataAssetsService : IAssetsService;

/// <inheritdoc cref="IAppDataAssetsService"/>
public sealed class AppDataAssetsService : FolderAssetsService, IAppDataAssetsService
{
    /// <summary> Instantiate a new AppDataAssetsService with a given path relative to the <see cref="Environment.SpecialFolder.ApplicationData"/> folder </summary>
    /// <param name="relativePath">The path relative to the <see cref="Environment.SpecialFolder.ApplicationData"/></param>
    public AppDataAssetsService(string relativePath)
        : base(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), relativePath)
    {
    }
}
