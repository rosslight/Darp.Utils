namespace Darp.Utils.Assets;

using Darp.Utils.Assets.Abstractions;

/// <summary> An interface for reading and writing to the AppData </summary>
public interface IAppDataAssetsService : IAssetsService;

/// <inheritdoc />
public sealed class AppDataAssetsService : IAppDataAssetsService
{
    /// <inheritdoc />
    public string BasePath { get; }

    /// <summary> Instantiate a new AppDataAssetsService with a given path relative to the AppData folder </summary>
    /// <param name="relativePath">The path relative to the AppData</param>
    public AppDataAssetsService(string relativePath)
    {
        BasePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), relativePath);
    }

    /// <inheritdoc />
    public bool Exists(string? path) => Path.Exists(Path.Join(BasePath, path));

    /// <inheritdoc />
    public Stream GetReadOnlySteam(string path) => File.OpenRead(Path.Join(BasePath, path));

    /// <inheritdoc />
    public Stream GetWriteOnlySteam(string path)
    {
        var joinedPath = Path.Join(BasePath, path);
        if (Exists(joinedPath))
        {
            return File.OpenWrite(joinedPath);
        }

        var directoryPath = Path.GetDirectoryName(joinedPath)
                            ?? throw new DirectoryNotFoundException("Could not get directory name");
        Directory.CreateDirectory(directoryPath);
        return File.OpenWrite(joinedPath);
    }
}
