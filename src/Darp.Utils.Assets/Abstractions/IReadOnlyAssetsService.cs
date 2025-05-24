namespace Darp.Utils.Assets.Abstractions;

using System.Diagnostics.CodeAnalysis;

/// <summary> An interface for reading from assets </summary>
public interface IReadOnlyAssetsService
{
    /// <summary> The base path of the assets </summary>
    public string BasePath { get; }

    /// <summary> Determines whether the specified asset exists. </summary>
    /// <param name="path">The path to check. Path should be relative to the <see cref="BasePath"/> </param>
    /// <returns>True if the asset could be found</returns>
    public bool Exists([NotNullWhen(true)] string? path);

    /// <summary> Yields relative file names matching the specified <paramref name="searchPattern"/> </summary>
    /// <param name="searchPattern"> A glob pattern for matching file paths </param>
    /// <returns> The paths of the files relative to the <see cref="BasePath"/> </returns>
    public IEnumerable<string> EnumerateFiles(string searchPattern = "*");

    /// <summary> Opens an existing asset for reading. </summary>
    /// <param name="path">The path to read from</param>
    /// <returns>A read-only Stream on the specified <paramref name="path"/>.</returns>
    public Stream GetReadOnlyStream(string path);
}
