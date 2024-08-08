namespace Darp.Utils.Assets.Implementations;

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
    public bool Exists(string uri) => Path.Exists(Path.Join(BasePath, uri));

    /// <inheritdoc />
    public Stream GetReadOnlySteam(string uri) => File.OpenRead(Path.Join(BasePath, uri));

    /// <inheritdoc />
    public Stream GetWriteOnlySteam(string uri)
    {
        var path = Path.Join(BasePath, uri);
        if (Exists(uri))
        {
            return File.OpenWrite(path);
        }

        var directoryPath = Path.GetDirectoryName(path)
                            ?? throw new DirectoryNotFoundException("Could not get directory name");
        Directory.CreateDirectory(directoryPath);
        return File.OpenWrite(path);
    }
}
