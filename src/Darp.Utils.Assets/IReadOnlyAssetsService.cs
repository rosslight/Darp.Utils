namespace Darp.Utils.Assets;

/// <summary> An interface for reading from assets </summary>
public interface IReadOnlyAssetsService
{
    /// <summary> The base path of the assets </summary>
    string BasePath { get; }
    /// <summary> Determines whether the specified asset exists. </summary>
    /// <param name="uri">The uri to check</param>
    /// <returns>True if the asset could be found</returns>
    bool Exists(string uri);
    /// <summary> Opens an existing asset for reading. </summary>
    /// <param name="uri">The uri to read from</param>
    /// <returns>A read-only Stream on the specified uri.</returns>
    Stream GetReadOnlySteam(string uri);
}
