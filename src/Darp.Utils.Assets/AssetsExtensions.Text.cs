namespace Darp.Utils.Assets;

/// <summary> Extensions for everything related to assets </summary>
public static partial class AssetsExtensions
{
    /// <summary>
    /// Asynchronously reads the bytes from the current stream and writes them to another stream, using a specified cancellation token. Both streams positions are advanced by the number of bytes copied.
    /// </summary>
    /// <param name="sourceAssetsService">The source assets service to be read from</param>
    /// <param name="sourcePath">The path to the source asset</param>
    /// <param name="targetAssetsService">The target assets service to be written to</param>
    /// <param name="targetPath">The path to the target position</param>
    /// <returns>A task that represents the asynchronous copy operation.</returns>
    public static void CopyTo(
        this IReadOnlyAssetsService sourceAssetsService,
        string sourcePath,
        IAssetsService targetAssetsService,
        string targetPath
    )
    {
        ArgumentNullException.ThrowIfNull(sourceAssetsService);
        ArgumentNullException.ThrowIfNull(targetAssetsService);
        using Stream sourceStream = sourceAssetsService.GetReadOnlyStream(sourcePath);
        using Stream targetStream = targetAssetsService.GetWriteOnlySteam(targetPath);

        targetStream.SetLength(0);
        sourceStream.CopyTo(targetStream);
    }

    /// <summary>
    /// Asynchronously reads the bytes from the current stream and writes them to another stream, using a specified cancellation token. Both streams positions are advanced by the number of bytes copied.
    /// </summary>
    /// <param name="sourceAssetsService">The source assets service to be read from</param>
    /// <param name="sourcePath">The path to the source asset</param>
    /// <param name="targetAssetsService">The target assets service to be written to</param>
    /// <param name="targetPath">The path to the target position</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
    /// <returns>A task that represents the asynchronous copy operation.</returns>
    public static async Task CopyToAsync(
        this IReadOnlyAssetsService sourceAssetsService,
        string sourcePath,
        IAssetsService targetAssetsService,
        string targetPath,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(sourceAssetsService);
        ArgumentNullException.ThrowIfNull(targetAssetsService);
        Stream sourceStream = sourceAssetsService.GetReadOnlyStream(sourcePath);
        Stream targetStream = targetAssetsService.GetWriteOnlySteam(targetPath);
        await using (sourceStream.ConfigureAwait(false))
        await using (targetStream.ConfigureAwait(false))
        {
            targetStream.SetLength(0);
            await sourceStream.CopyToAsync(targetStream, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Asynchronously reads the bytes from the current stream and writes them to another stream, using a specified cancellation token. Both streams positions are advanced by the number of bytes copied.
    /// </summary>
    /// <param name="sourceAssetsService">The source assets service to be read from</param>
    /// <param name="sourcePath">The path to the source asset</param>
    /// <param name="targetAssetsService">The target file assets service to be written to</param>
    /// <param name="targetPath">The path to the target position</param>
    /// <param name="fileAttributes"> The file attributes to be used for each file </param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
    /// <returns>A task that represents the asynchronous copy operation.</returns>
    public static async Task CopyToAsync(
        this IReadOnlyAssetsService sourceAssetsService,
        string sourcePath,
        IFolderAssetsService targetAssetsService,
        string targetPath,
        FileAttributes fileAttributes,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(sourceAssetsService);
        ArgumentNullException.ThrowIfNull(targetAssetsService);
        Stream sourceStream = sourceAssetsService.GetReadOnlyStream(sourcePath);
        Stream targetStream = targetAssetsService.GetWriteOnlySteam(targetPath, fileAttributes);
        await using (sourceStream.ConfigureAwait(false))
        await using (targetStream.ConfigureAwait(false))
        {
            targetStream.SetLength(0);
            await sourceStream.CopyToAsync(targetStream, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary> Asynchronously reads a character string from the assets service. </summary>
    /// <param name="targetAssetsService">The target assets service to be written to</param>
    /// <param name="path">The path to the file relative to the target assets service</param>
    public static string ReadText(this IReadOnlyAssetsService targetAssetsService, string path)
    {
        ArgumentNullException.ThrowIfNull(targetAssetsService);
        using Stream stream = targetAssetsService.GetReadOnlyStream(path);
        using var writer = new StreamReader(stream);
        return writer.ReadToEnd();
    }

    /// <summary> Asynchronously reads a character string from the assets service. </summary>
    /// <param name="targetAssetsService">The target assets service to be written to</param>
    /// <param name="path">The path to the file relative to the target assets service</param>
    /// <param name="cancellationToken"> The cancellation token to cancel the operation </param>
    public static async Task<string> ReadTextAsync(
        this IReadOnlyAssetsService targetAssetsService,
        string path,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(targetAssetsService);
        Stream stream = targetAssetsService.GetReadOnlyStream(path);
        var writer = new StreamReader(stream);
        await using (stream.ConfigureAwait(false))
        using (writer)
        {
            return await writer.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary> Asynchronously writes a character string to the assets service. </summary>
    /// <param name="targetAssetsService">The target assets service to be written to</param>
    /// <param name="path">The path to the file relative to the target assets service</param>
    /// <param name="content"> The content to be written to the asset </param>
    public static void WriteText(this IAssetsService targetAssetsService, string path, string content)
    {
        ArgumentNullException.ThrowIfNull(targetAssetsService);
        using Stream stream = targetAssetsService.GetWriteOnlySteam(path);
        using var writer = new StreamWriter(stream);
        stream.SetLength(0);
        writer.Write(content);
    }

    /// <summary> Asynchronously writes a character string to the assets service. </summary>
    /// <param name="targetAssetsService">The target assets service to be written to</param>
    /// <param name="path">The path to the file relative to the target assets service</param>
    /// <param name="content"> The content to be written to the asset </param>
    /// <param name="cancellationToken"> The cancellation token to cancel the operation </param>
    public static async Task WriteTextAsync(
        this IAssetsService targetAssetsService,
        string path,
        string content,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(targetAssetsService);
        Stream stream = targetAssetsService.GetWriteOnlySteam(path);
        var writer = new StreamWriter(stream);
        await using (stream.ConfigureAwait(false))
        await using (writer.ConfigureAwait(false))
        {
            stream.SetLength(0);
            await writer.WriteAsync(content.AsMemory(), cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary> Asynchronously writes a character string to the assets service. </summary>
    /// <param name="targetAssetsService">The target assets service to be written to</param>
    /// <param name="path">The path to the file relative to the target assets service</param>
    /// <param name="content"> The content to be written to the asset </param>
    /// <param name="fileAttributes"> The file attributes to be used for each file </param>
    /// <param name="cancellationToken"> The cancellation token to cancel the operation </param>
    public static async Task WriteTextAsync(
        this IFolderAssetsService targetAssetsService,
        string path,
        string content,
        FileAttributes fileAttributes,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(targetAssetsService);
        Stream stream = targetAssetsService.GetWriteOnlySteam(path, fileAttributes);
        var writer = new StreamWriter(stream);
        await using (stream.ConfigureAwait(false))
        await using (writer.ConfigureAwait(false))
        {
            stream.SetLength(0);
            await writer.WriteAsync(content.AsMemory(), cancellationToken).ConfigureAwait(false);
        }
    }
}
