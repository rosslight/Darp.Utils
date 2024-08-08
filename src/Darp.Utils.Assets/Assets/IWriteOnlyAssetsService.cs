namespace Darp.Utils.Assets.Assets;

/// <summary> An interface for writing to assets </summary>
public interface IWriteOnlyAssetsService
{
    /// <summary> Opens an asset for writing and creates it if not existent. </summary>
    /// <param name="uri">The uri to write to</param>
    /// <returns>A write-only Stream on the specified uri.</returns>
    Stream GetWriteOnlySteam(string uri);
}
