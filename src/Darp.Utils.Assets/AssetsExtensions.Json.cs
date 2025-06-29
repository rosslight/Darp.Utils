namespace Darp.Utils.Assets;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

/// <summary> Extensions for everything related to assets </summary>
public static partial class AssetsExtensions
{
    private const string JsonRequiresUnreferencedCodeMessage =
        "JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved.";
    private const string JsonRequiresDynamicCodeMessage =
        "JSON serialization and deserialization might require types that cannot be statically analyzed and might need runtime code generation. Use System.Text.Json source generation for native AOT applications.";

    /// <summary> Deserialize a json from a given asset </summary>
    /// <param name="sourceAssetsService">The source assets service to be read from</param>
    /// <param name="path">The path to the file relative to the source assets service</param>
    /// <param name="options">Options to control the behavior during reading.</param>
    /// <typeparam name="TValue">The target type of the JSON value.</typeparam>
    /// <returns>A TValue representation of the JSON value.</returns>
    /// <exception cref="Exception">Deserialization returned null</exception>
    /// <exception cref="NotSupportedException">There is no compatible <see cref="System.Text.Json.Serialization.JsonConverter" /> for <typeparamref name="TValue" /> or its serializable members.</exception>
    [RequiresUnreferencedCode(JsonRequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(JsonRequiresDynamicCodeMessage)]
    public static TValue DeserializeJson<TValue>(
        this IReadOnlyAssetsService sourceAssetsService,
        string path,
        JsonSerializerOptions? options = null
    )
    {
        options ??= JsonSerializerOptions.Default;
        var typeInfo = (JsonTypeInfo<TValue>)options.GetTypeInfo(typeof(TValue));
        return sourceAssetsService.DeserializeJson(path, typeInfo);
    }

    /// <summary> Deserialize a json from a given asset </summary>
    /// <param name="sourceAssetsService">The source assets service to be read from</param>
    /// <param name="path">The path to the file relative to the source assets service</param>
    /// <param name="typeInfo">Metadata about the type to convert.</param>
    /// <typeparam name="TValue">The target type of the JSON value.</typeparam>
    /// <returns>A TValue representation of the JSON value.</returns>
    /// <exception cref="Exception">Deserialization returned null</exception>
    /// <exception cref="NotSupportedException">There is no compatible <see cref="System.Text.Json.Serialization.JsonConverter" /> for <typeparamref name="TValue" /> or its serializable members.</exception>
    public static TValue DeserializeJson<TValue>(
        this IReadOnlyAssetsService sourceAssetsService,
        string path,
        JsonTypeInfo<TValue> typeInfo
    )
    {
        ArgumentNullException.ThrowIfNull(sourceAssetsService);
        using Stream stream = sourceAssetsService.GetReadOnlyStream(path);
        return JsonSerializer.Deserialize(stream, typeInfo) ?? throw new JsonException("Deserialization returned null");
    }

    /// <summary> Deserialize a json from a given asset </summary>
    /// <param name="sourceAssetsService">The source assets service to be read from</param>
    /// <param name="path">The path to the file relative to the source assets service</param>
    /// <param name="options">Options to control the behavior during reading.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the read operation.</param>
    /// <typeparam name="TValue">The target type of the JSON value.</typeparam>
    /// <returns>A TValue representation of the JSON value.</returns>
    /// <exception cref="Exception">Deserialization returned null</exception>
    /// <exception cref="NotSupportedException">There is no compatible <see cref="System.Text.Json.Serialization.JsonConverter" /> for <typeparamref name="TValue" /> or its serializable members.</exception>
    [RequiresUnreferencedCode(JsonRequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(JsonRequiresDynamicCodeMessage)]
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
    /// <typeparam name="TValue">The target type of the JSON value.</typeparam>
    /// <exception cref="NotSupportedException">There is no compatible <see cref="System.Text.Json.Serialization.JsonConverter" /> for <typeparamref name="TValue" /> or its serializable members.</exception>
    /// <returns>A task that represents the asynchronous copy operation.</returns>
    [RequiresUnreferencedCode(JsonRequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(JsonRequiresDynamicCodeMessage)]
    public static void SerializeJson<TValue>(
        this IAssetsService targetAssetsService,
        string path,
        TValue value,
        JsonSerializerOptions? options = null
    )
    {
        options ??= JsonSerializerOptions.Default;
        var typeInfo = (JsonTypeInfo<TValue>)options.GetTypeInfo(typeof(TValue));
        targetAssetsService.SerializeJson(path, value, typeInfo);
    }

    /// <summary> Serialize a json from a given value and write it to the asset </summary>
    /// <param name="targetAssetsService">The target assets service to be written to</param>
    /// <param name="path">The path to the file relative to the target assets service</param>
    /// <param name="value">The value to convert and write.</param>
    /// <param name="typeInfo">Metadata about the type to convert.</param>
    /// <typeparam name="TValue">The target type of the JSON value.</typeparam>
    /// <exception cref="NotSupportedException">There is no compatible <see cref="System.Text.Json.Serialization.JsonConverter" /> for <typeparamref name="TValue" /> or its serializable members.</exception>
    /// <returns>A task that represents the asynchronous copy operation.</returns>
    public static void SerializeJson<TValue>(
        this IAssetsService targetAssetsService,
        string path,
        TValue value,
        JsonTypeInfo<TValue> typeInfo
    )
    {
        ArgumentNullException.ThrowIfNull(targetAssetsService);
        using Stream stream = targetAssetsService.GetWriteOnlySteam(path);
        stream.SetLength(0);
        JsonSerializer.Serialize(stream, value, typeInfo);
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
    [RequiresUnreferencedCode(JsonRequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(JsonRequiresDynamicCodeMessage)]
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
}
