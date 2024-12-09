namespace Darp.Utils.Assets;

using System.Reflection;
using System.Text.RegularExpressions;
using Abstractions;
using GlobExpressions;

/// <summary> An interface for reading from the embedded files of a specific assembly </summary>
public interface IEmbeddedResourceAssetsService : IReadOnlyAssetsService;

/// <inheritdoc />
/// <param name="assembly">The assembly to be used</param>
public class EmbeddedResourceAssetsService(Assembly assembly) : IEmbeddedResourceAssetsService
{
    private readonly Assembly _assembly = assembly;
    private Regex? _noBasePathRegex;

    /// <inheritdoc />
    public string BasePath { get; } = (assembly.GetName().Name ?? "").Replace('.', '/');

    private string GetPath(string path)
    {
        var uri = Path.Join(BasePath, path);
        uri = uri.Replace('/', '.');
        uri = uri.Replace('\\', '.');
        return uri;
    }

    /// <inheritdoc />
    public IEnumerable<string> EnumerateFiles(string searchPattern)
    {
        // Lazy initialization of regex. Can be cached as BasePath is constant for each class
        _noBasePathRegex ??= new Regex(@$"^\/*{Regex.Escape(BasePath)}", RegexOptions.Compiled);
        var glob = new Glob($"{BasePath}/{searchPattern}");
        foreach (var manifestResourceName in _assembly.GetManifestResourceNames())
        {
            var fileName = Path.GetFileNameWithoutExtension(manifestResourceName);
            fileName = fileName.Replace('.', '/');
            fileName = $"{fileName}{Path.GetExtension(manifestResourceName)}";
            if (glob.IsMatch(fileName))
                yield return _noBasePathRegex.Replace(fileName, "").TrimStart('/');
        }
    }

    /// <inheritdoc />
    public Stream GetReadOnlySteam(string path)
    {
        var uri = GetPath(path);
        return _assembly.GetManifestResourceStream(uri)
            ?? throw new IOException($"Could not find {uri} in resource stream");
    }

    /// <inheritdoc />
    public bool Exists(string? path)
    {
        if (path is null)
            return false;
        var uri = GetPath(path);
        return _assembly.GetManifestResourceInfo(uri) is not null;
    }
}
