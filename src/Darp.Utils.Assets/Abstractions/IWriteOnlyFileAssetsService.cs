namespace Darp.Utils.Assets.Abstractions;

/// <summary> An interface for writing to file assets </summary>
public interface IWriteOnlyFileAssetsService : IWriteOnlyAssetsService
{
    /// <summary> Opens an asset for writing and creates it if not existent. </summary>
    /// <param name="path"> The path to write to </param>
    /// <param name="fileAttributes"> The file attributes to be used while writing </param>
    /// <returns> A write-only Stream on the specified path. </returns>
    public Stream GetWriteOnlySteam(string path, FileAttributes fileAttributes);
}
