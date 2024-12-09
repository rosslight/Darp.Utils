namespace Darp.Utils.Assets.Abstractions;

/// <summary> An interface for writing to assets </summary>
public interface IWriteOnlyAssetsService
{
    /// <summary> Opens an asset for writing and creates it if not existent. </summary>
    /// <param name="path">The path to write to</param>
    /// <returns>A write-only Stream on the specified path.</returns>
    public Stream GetWriteOnlySteam(string path);
}
