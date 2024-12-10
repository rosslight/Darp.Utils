namespace Darp.Utils.Assets;

using Abstractions;

/// <summary> An interface for reading from and writing to the <see cref="AppDomain.CurrentDomain"/>.<see cref="AppDomain.BaseDirectory"/> </summary>
public interface IBaseDirectoryAssetsService : IAssetsService, IWriteOnlyFileAssetsService;

/// <inheritdoc cref="IProgramDataAssetsService"/>
/// <summary> Instantiate a new ProgramDataAssetsService with a given path relative to the <see cref="AppDomain.CurrentDomain"/>.<see cref="AppDomain.BaseDirectory"/> folder </summary>
public sealed class BaseDirectoryAssetsService()
    : FolderAssetsService(AppDomain.CurrentDomain.BaseDirectory, string.Empty),
        IBaseDirectoryAssetsService;
