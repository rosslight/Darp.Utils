namespace Darp.Utils.TestRail.Models;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary> The model of the section </summary>
public sealed record GetSection
{
    /// <summary> The level in the section hierarchy of the test suite </summary>
    public int Depth { get; init; }

    /// <summary> The description of the section </summary>
    public string? Description { get; init; }

    /// <summary> The order in the test suite </summary>
    public int DisplayOrder { get; init; }

    /// <summary> The unique ID of the section </summary>
    public required SectionId Id { get; init; }

    /// <summary> The ID of the parent section in the test suite </summary>
    public SectionId? ParentId { get; init; }

    /// <summary> The name of the section </summary>
    public required string Name { get; init; }

    /// <summary> The ID of the test suite this section belongs to </summary>
    public SuiteId SuiteId { get; init; }

    /// <summary> User defined properties </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement> Properties { get; set; } = [];
}
