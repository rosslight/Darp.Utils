namespace Darp.Utils.Assets;

using Abstractions;

/// <summary> An interface for reading from and writing to the <see cref="Environment.SpecialFolder.CommonApplicationData"/> </summary>
public interface IProgramDataAssetsService : IAssetsService, IWriteOnlyFileAssetsService;

/// <inheritdoc cref="IProgramDataAssetsService"/>
/// <summary> Instantiate a new ProgramDataAssetsService with a given path relative to the <see cref="Environment.SpecialFolder.CommonApplicationData"/> folder </summary>
/// <param name="relativePath">The path relative to the <see cref="Environment.SpecialFolder.CommonApplicationData"/></param>
public sealed class ProgramDataAssetsService(string relativePath)
    : FolderAssetsService(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), relativePath),
        IProgramDataAssetsService;
