namespace Darp.Utils.Assets;

using Darp.Utils.Assets.Abstractions;

/// <summary> An interface for reading from and writing to a predefined folder </summary>
public interface IFolderAssetsService : IAssetsService;

/// <inheritdoc />
/// <summary> Instantiate a new <see cref="FolderAssetsService"/> with a given path relative to the <paramref name="folderPath"/> </summary>
/// <param name="folderPath">The path to the folder </param>
/// <param name="relativePath">The path relative to the <paramref name="folderPath"/></param>
public class FolderAssetsService(string folderPath, string relativePath) : IFolderAssetsService
{
    /// <inheritdoc />
    public string BasePath { get; } = Path.Join(folderPath, relativePath);

    /// <inheritdoc />
    public bool Exists(string? path) => Path.Exists(Path.Join(BasePath, path));

    /// <inheritdoc />
    public Stream GetReadOnlySteam(string path) => File.OpenRead(Path.Join(BasePath, path));

    /// <inheritdoc />
    public Stream GetWriteOnlySteam(string path)
    {
        var joinedPath = Path.Join(BasePath, path);
        if (Exists(path))
        {
            return File.OpenWrite(joinedPath);
        }

        var directoryPath =
            Path.GetDirectoryName(joinedPath) ?? throw new DirectoryNotFoundException("Could not get directory name");
        Directory.CreateDirectory(directoryPath);
        return File.OpenWrite(joinedPath);
    }
}
