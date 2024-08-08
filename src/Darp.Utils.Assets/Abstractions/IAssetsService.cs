namespace Darp.Utils.Assets.Abstractions;

/// <summary> An interface for reading and writing to assets </summary>
public interface IAssetsService : IReadOnlyAssetsService, IWriteOnlyAssetsService;
