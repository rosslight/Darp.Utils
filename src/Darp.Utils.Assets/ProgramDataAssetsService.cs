namespace Darp.Utils.Assets;

using Darp.Utils.Assets.Abstractions;

/// <summary> An interface for reading from and writing to the <see cref="Environment.SpecialFolder.CommonApplicationData"/> </summary>
public interface IProgramDataAssetsService : IAssetsService;

/// <inheritdoc cref="IProgramDataAssetsService"/>
public sealed class ProgramDataAssetsService : FolderAssetsService, IProgramDataAssetsService
{
    /// <summary> Instantiate a new ProgramDataAssetsService with a given path relative to the <see cref="Environment.SpecialFolder.CommonApplicationData"/> folder </summary>
    /// <param name="relativePath">The path relative to the <see cref="Environment.SpecialFolder.CommonApplicationData"/></param>
    public ProgramDataAssetsService(string relativePath)
        : base(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), relativePath)
    {
    }
}
