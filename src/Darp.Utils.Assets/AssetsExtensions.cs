namespace Darp.Utils.Assets;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

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
    /// <exception cref="NotSupportedException">There is no compatible <see cref="System.Text.Json.Serialization.JsonConverter" /> for <typeparamref name="TValue" /> or its serializable members.</exception>
    [RequiresUnreferencedCode(
        "JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved."
    )]
    [RequiresDynamicCode(
        "JSON serialization and deserialization might require types that cannot be statically analyzed and might need runtime code generation. Use System.Text.Json source generation for native AOT applications."
    )]
    public static Task<TValue> DeserializeJsonAsync<TValue>(
        this IReadOnlyAssetsService sourceAssetsService,
        string path,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        options ??= JsonSerializerOptions.Default;
        var typeInfo = (JsonTypeInfo<TValue>)options.GetTypeInfo(typeof(TValue));
        return sourceAssetsService.DeserializeJsonAsync(path, typeInfo, cancellationToken);
    }

    /// <summary> Deserialize a json from a given asset </summary>
    /// <param name="sourceAssetsService">The source assets service to be read from</param>
    /// <param name="path">The path to the file relative to the source assets service</param>
    /// <param name="typeInfo">Metadata about the type to convert.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the read operation.</param>
    /// <typeparam name="TValue">The target type of the JSON value.</typeparam>
    /// <returns>A TValue representation of the JSON value.</returns>
    /// <exception cref="Exception">Deserialization returned null</exception>
    /// <exception cref="NotSupportedException">There is no compatible <see cref="System.Text.Json.Serialization.JsonConverter" /> for <typeparamref name="TValue" /> or its serializable members.</exception>
    public static async Task<TValue> DeserializeJsonAsync<TValue>(
        this IReadOnlyAssetsService sourceAssetsService,
        string path,
        JsonTypeInfo<TValue> typeInfo,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(sourceAssetsService);
        Stream stream = sourceAssetsService.GetReadOnlyStream(path);
        await using (stream.ConfigureAwait(false))
        {
            return await JsonSerializer.DeserializeAsync(stream, typeInfo, cancellationToken).ConfigureAwait(false)
                ?? throw new JsonException("Deserialization returned null");
        }
    }

    /// <summary> Serialize a json from a given value and write it to the asset </summary>
    /// <param name="targetAssetsService">The target assets service to be written to</param>
    /// <param name="path">The path to the file relative to the target assets service</param>
    /// <param name="value">The value to convert and write.</param>
    /// <param name="options">Options to control serialization behavior</param>
    /// <param name="cancellationToken">A token that may be used to cancel the read operation.</param>
    /// <typeparam name="TValue">The target type of the JSON value.</typeparam>
    /// <exception cref="NotSupportedException">There is no compatible <see cref="System.Text.Json.Serialization.JsonConverter" /> for <typeparamref name="TValue" /> or its serializable members.</exception>
    /// <returns>A task that represents the asynchronous copy operation.</returns>
    [RequiresUnreferencedCode(
        "JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved."
    )]
    [RequiresDynamicCode(
        "JSON serialization and deserialization might require types that cannot be statically analyzed and might need runtime code generation. Use System.Text.Json source generation for native AOT applications."
    )]
    public static Task SerializeJsonAsync<TValue>(
        this IAssetsService targetAssetsService,
        string path,
        TValue value,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        options ??= JsonSerializerOptions.Default;
        var typeInfo = (JsonTypeInfo<TValue>)options.GetTypeInfo(typeof(TValue));
        return targetAssetsService.SerializeJsonAsync(path, value, typeInfo, cancellationToken);
    }

    /// <summary> Serialize a json from a given value and write it to the asset </summary>
    /// <param name="targetAssetsService">The target assets service to be written to</param>
    /// <param name="path">The path to the file relative to the target assets service</param>
    /// <param name="value">The value to convert and write.</param>
    /// <param name="typeInfo">Metadata about the type to convert.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the read operation.</param>
    /// <typeparam name="TValue">The target type of the JSON value.</typeparam>
    /// <exception cref="NotSupportedException">There is no compatible <see cref="System.Text.Json.Serialization.JsonConverter" /> for <typeparamref name="TValue" /> or its serializable members.</exception>
    /// <returns>A task that represents the asynchronous copy operation.</returns>
    public static async Task SerializeJsonAsync<TValue>(
        this IAssetsService targetAssetsService,
        string path,
        TValue value,
        JsonTypeInfo<TValue> typeInfo,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(targetAssetsService);
        Stream stream = targetAssetsService.GetWriteOnlySteam(path);
        await using (stream.ConfigureAwait(false))
        {
            stream.SetLength(0);
            await JsonSerializer.SerializeAsync(stream, value, typeInfo, cancellationToken).ConfigureAwait(false);
        }
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
