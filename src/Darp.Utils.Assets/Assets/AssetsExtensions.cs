namespace Darp.Utils.Assets.Assets;

using System.Text.Json;

/// <summary> Extensions for everything related to assets </summary>
public static class AssetsExtensions
{
    /// <summary> Deserialize a json from a given asset </summary>
    /// <param name="sourceAssetsService">The source assets service to be read from</param>
    /// <param name="uri">The uri of the file relative to the source assets service</param>
    /// <param name="options">Options to control the behavior during reading.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the read operation.</param>
    /// <typeparam name="TValue">The target type of the JSON value.</typeparam>
    /// <returns>A TValue representation of the JSON value.</returns>
    /// <exception cref="Exception">Deserialization returned null</exception>
    /// <exception cref="T:System.NotSupportedException">There is no compatible <see cref="System.Text.Json.Serialization.JsonConverter" /> for <typeparamref name="TValue" /> or its serializable members.</exception>
    public static async Task<TValue> DeserializeJsonAsync<TValue>(this IReadOnlyAssetsService sourceAssetsService,
        string uri,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        await using Stream stream = sourceAssetsService.GetReadOnlySteam(uri);
        return await JsonSerializer.DeserializeAsync<TValue>(stream, options, cancellationToken)
               ?? throw new Exception("Deserialization returned null");
    }

    /// <summary> Serialize a json from a given value and write it to the asset </summary>
    /// <param name="targetAssetsService">The target assets service to be written to</param>
    /// <param name="uri">The uri of the file relative to the target assets service</param>
    /// <param name="value">The value to convert and write.</param>
    /// <param name="options">Options to control serialization behavior</param>
    /// <param name="cancellationToken">A token that may be used to cancel the read operation.</param>
    /// <typeparam name="TValue">The target type of the JSON value.</typeparam>
    /// <exception cref="T:System.NotSupportedException">There is no compatible <see cref="System.Text.Json.Serialization.JsonConverter" /> for <typeparamref name="TValue" /> or its serializable members.</exception>
    /// <returns>A task that represents the asynchronous copy operation.</returns>
    public static async Task SerializeJsonAsync<TValue>(this IWriteOnlyAssetsService targetAssetsService,
        string uri,
        TValue value,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        await using Stream stream = targetAssetsService.GetWriteOnlySteam(uri);
        stream.SetLength(0);
        await JsonSerializer.SerializeAsync(stream, value, options, cancellationToken);
    }

    /// <summary>
    /// Asynchronously reads the bytes from the current stream and writes them to another stream, using a specified cancellation token. Both streams positions are advanced by the number of bytes copied.
    /// </summary>
    /// <param name="sourceAssetsService">The source assets service to be read from</param>
    /// <param name="sourceUri">The uri of the source asset</param>
    /// <param name="targetAssetsService">The target assets service to be written to</param>
    /// <param name="targetUri">The uri of the target position</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
    /// <returns>A task that represents the asynchronous copy operation.</returns>
    public static async Task CopyToAsync(this IReadOnlyAssetsService sourceAssetsService,
        string sourceUri,
        IWriteOnlyAssetsService targetAssetsService,
        string targetUri,
        CancellationToken cancellationToken = default)
    {
        await using Stream sourceStream = sourceAssetsService.GetReadOnlySteam(sourceUri);
        await using Stream targetStream = targetAssetsService.GetWriteOnlySteam(targetUri);
        await sourceStream.CopyToAsync(targetStream, cancellationToken);
    }
}
