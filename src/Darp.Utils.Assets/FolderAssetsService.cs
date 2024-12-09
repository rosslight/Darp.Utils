namespace Darp.Utils.Assets;

using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Darp.Utils.Assets.Abstractions;
using GlobExpressions;

/// <summary> An interface for reading from and writing to a predefined folder </summary>
public interface IFolderAssetsService : IAssetsService;

/// <inheritdoc />
/// <summary> Instantiate a new <see cref="FolderAssetsService"/> with a given path relative to the <paramref name="folderPath"/> </summary>
/// <param name="folderPath">The path to the folder </param>
/// <param name="relativePath">The path relative to the <paramref name="folderPath"/></param>
public class FolderAssetsService(string folderPath, string relativePath) : IFolderAssetsService
{
    private Regex? _noBasePathRegex;

    /// <inheritdoc />
    public string BasePath { get; } = Path.Join(folderPath, relativePath).Replace('\\', '/');

    /// <inheritdoc />
    public bool Exists([NotNullWhen(true)] string? path) => Path.Exists(Path.Join(BasePath, path));

    /// <inheritdoc />
    public IEnumerable<string> EnumerateFiles(string searchPattern = "*")
    {
        // Lazy initialization of regex. Can be cached as BasePath is constant for each class
        _noBasePathRegex ??= new Regex(@$"^\/*{Regex.Escape(BasePath)}", RegexOptions.Compiled);
        var glob = new Glob($"{BasePath}/{searchPattern}");
        foreach (var filePath in Directory.EnumerateFiles(BasePath, "*", SearchOption.AllDirectories))
        {
            if (!glob.IsMatch(filePath))
                continue;
            var xx = filePath.Replace('\\', '/');
            xx = _noBasePathRegex.Replace(xx, "");
            if (xx.StartsWith('/'))
                xx = xx[1..];
            yield return xx;
        }
    }

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
