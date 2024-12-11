namespace Darp.Utils.TestRail.Models;

using System.Text.Json;
using System.Text.Json.Serialization;
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
[JsonSerializable(typeof(GetResults))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
    public static SourceGenerationContext CustomOptions { get; } = CreateWithCustomOptions();

    private static SourceGenerationContext CreateWithCustomOptions()
    {
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
        options.Converters.Add(new TestRailTimespanJsonConverter());
        options.Converters.Add(new UnixToDateTimeOffsetConverter());
        return new SourceGenerationContext(options);
    }
}
