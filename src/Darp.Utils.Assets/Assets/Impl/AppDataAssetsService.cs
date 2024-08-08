namespace Darp.Utils.Assets.Assets.Impl;

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
        string path = Path.Join(BasePath, uri);
        if (!Exists(uri))
        {
            string directoryPath = Path.GetDirectoryName(path) ?? throw new Exception("Could not get directory name");
            Directory.CreateDirectory(directoryPath);
        }
        return File.OpenWrite(path);
    }
}
