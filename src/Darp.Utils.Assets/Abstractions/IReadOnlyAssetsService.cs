namespace Darp.Utils.Assets.Abstractions;

/// <summary> An interface for reading from assets </summary>
public interface IReadOnlyAssetsService
{
    /// <summary> The base path of the assets </summary>
    string BasePath { get; }

    /// <summary> Determines whether the specified asset exists. </summary>
    /// <param name="path">The path to check</param>
    /// <returns>True if the asset could be found</returns>
    bool Exists(string? path);

    /// <summary> Opens an existing asset for reading. </summary>
    /// <param name="path">The path to read from</param>
    /// <returns>A read-only Stream on the specified <paramref name="path"/>.</returns>
    Stream GetReadOnlySteam(string path);
}
