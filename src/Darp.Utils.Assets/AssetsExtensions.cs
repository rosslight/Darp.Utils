namespace Darp.Utils.Assets;

using System.Text.Json;
using Abstractions;

/// <summary> Extensions for everything related to assets </summary>
public static class AssetsExtensions
{
    /// <summary> Deserialize a json from a given asset </summary>
    /// <param name="sourceAssetsService">The source assets service to be read from</param>
    /// <param name="path">The path to the file relative to the source assets service</param>
    /// <param name="options">Options to control the behavior during reading.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the read operation.</param>
    /// <typeparam name="TValue">The target type of the JSON value.</typeparam>
    /// <returns>A TValue representation of the JSON value.</returns>
    /// <exception cref="Exception">Deserialization returned null</exception>
    /// <exception cref="System.NotSupportedException">There is no compatible <see cref="System.Text.Json.Serialization.JsonConverter" /> for <typeparamref name="TValue" /> or its serializable members.</exception>
    public static async Task<TValue> DeserializeJsonAsync<TValue>(this IReadOnlyAssetsService sourceAssetsService,
        string path,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sourceAssetsService);
        await using Stream stream = sourceAssetsService.GetReadOnlySteam(path);
        return await JsonSerializer.DeserializeAsync<TValue>(stream, options, cancellationToken)
               ?? throw new JsonException("Deserialization returned null");
    }

    /// <summary> Serialize a json from a given value and write it to the asset </summary>
    /// <param name="targetAssetsService">The target assets service to be written to</param>
    /// <param name="path">The path to the file relative to the target assets service</param>
    /// <param name="value">The value to convert and write.</param>
    /// <param name="options">Options to control serialization behavior</param>
    /// <param name="cancellationToken">A token that may be used to cancel the read operation.</param>
    /// <typeparam name="TValue">The target type of the JSON value.</typeparam>
    /// <exception cref="System.NotSupportedException">There is no compatible <see cref="System.Text.Json.Serialization.JsonConverter" /> for <typeparamref name="TValue" /> or its serializable members.</exception>
    /// <returns>A task that represents the asynchronous copy operation.</returns>
    public static async Task SerializeJsonAsync<TValue>(this IWriteOnlyAssetsService targetAssetsService,
        string path,
        TValue value,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(targetAssetsService);
        await using Stream stream = targetAssetsService.GetWriteOnlySteam(path);
        stream.SetLength(0);
        await JsonSerializer.SerializeAsync(stream, value, options, cancellationToken);
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
    public static async Task CopyToAsync(this IReadOnlyAssetsService sourceAssetsService,
        string sourcePath,
        IWriteOnlyAssetsService targetAssetsService,
        string targetPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sourceAssetsService);
        ArgumentNullException.ThrowIfNull(targetAssetsService);
        await using Stream sourceStream = sourceAssetsService.GetReadOnlySteam(sourcePath);
        await using Stream targetStream = targetAssetsService.GetWriteOnlySteam(targetPath);
        await sourceStream.CopyToAsync(targetStream, cancellationToken);
    }
}
