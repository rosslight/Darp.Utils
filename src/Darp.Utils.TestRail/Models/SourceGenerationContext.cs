namespace Darp.Utils.TestRail.Models;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Json;

[JsonSerializable(typeof(Unit))]
[JsonSerializable(typeof(GetTests))]
[JsonSerializable(typeof(GetRuns))]
[JsonSerializable(typeof(UpdateCaseRequest))]
[JsonSerializable(typeof(GetCaseResponse))]
[JsonSerializable(typeof(IEnumerable<GetSuite>))]
[JsonSerializable(typeof(GetSections))]
[JsonSerializable(typeof(GetCases))]
[JsonSerializable(typeof(GetProjects))]
[JsonSerializable(typeof(GetMilestone))]
[JsonSerializable(typeof(GetUser))]
[JsonSerializable(typeof(AddResultRequest))]
[JsonSerializable(typeof(GetResultResponse))]
internal sealed partial class SourceGenerationContext : JsonSerializerContext
{
    public static readonly JsonSerializerOptions CustomOptions = CreateCustomOptions();
    public static SourceGenerationContext TestRail { get; } = new(new JsonSerializerOptions(CustomOptions));

    private static JsonSerializerOptions CreateCustomOptions()
    {
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
        options.Converters.Add(new TestRailTimespanJsonConverter());
        options.Converters.Add(new UnixToDateTimeOffsetConverter());
        return options;
    }

    [RequiresUnreferencedCode(
        "JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved."
    )]
    [RequiresDynamicCode(
        "JSON serialization and deserialization might require types that cannot be statically analyzed and might need runtime code generation. Use System.Text.Json source generation for native AOT applications."
    )]
    public static JsonTypeInfo<T> CreateDefaultJsonTypeInfo<T>()
    {
        var options = new JsonSerializerOptions(CustomOptions) { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };
        return (JsonTypeInfo<T>)options.GetTypeInfo(typeof(T));
    }
}
