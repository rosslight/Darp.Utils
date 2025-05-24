namespace Darp.Utils.Assets;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Abstractions;
using GlobExpressions;

/// <summary> An In-Memory Assets service </summary>
public class MemoryAssetsService(string basePath) : IAssetsService
{
    private readonly ConcurrentDictionary<string, byte[]> _storage = new(StringComparer.Ordinal);

    /// <inheritdoc />
    public string BasePath { get; } = basePath;

    /// <inheritdoc />
    public bool Exists([NotNullWhen(true)] string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        return _storage.ContainsKey(Normalize(path));
    }

    /// <inheritdoc />
    public IEnumerable<string> EnumerateFiles(string searchPattern = "*")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPattern);

        var glob = new Glob(searchPattern);
        return _storage.Keys.Where(key => glob.IsMatch(Path.GetFileName(key))).ToArray();
    }

    /// <inheritdoc />
    public Stream GetReadOnlySteam(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var key = Normalize(path);
        if (!_storage.TryGetValue(key, out var data))
            throw new FileNotFoundException($"File not found: {path}", path);

        // Return a new read-only MemoryStream
        return new MemoryStream(data, writable: false);
    }

    /// <inheritdoc />
    public Stream GetWriteOnlySteam(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var key = Normalize(path);
        // Return a stream that commits on dispose
        return new CommitOnDisposeStream(key, this);
    }

    private void Commit(string pathKey, byte[] data)
    {
        _storage[pathKey] = data;
    }

    private static string Normalize(string path) => path.Replace('\\', '/').TrimStart('/');

    private sealed class CommitOnDisposeStream(string pathKey, MemoryAssetsService parent) : MemoryStream
    {
        private readonly string _pathKey = pathKey;
        private readonly MemoryAssetsService _parent = parent;
        private bool _committed;

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_committed)
            {
                // Commit data to parent storage
                var data = ToArray();
                _parent.Commit(_pathKey, data);
                _committed = true;
            }
            base.Dispose(disposing);
        }
    }
}
