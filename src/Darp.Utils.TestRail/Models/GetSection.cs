namespace Darp.Utils.TestRail.Models;

using System.Text.Json;
using System.Text.Json.Serialization;

public sealed record GetSection(
    SectionId Id,
    string Name,
    string Description,
    SuiteId SuiteId,
    int Depth,
    int DisplayOrder,
    SectionId? ParentId = null
)
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement> Properties { get; set; } = [];
}
