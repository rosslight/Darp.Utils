namespace Darp.Utils.TestRail.Models;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public sealed record AddResultRequest
{
    public required StatusId StatusId { get; init; }
    public required string Comment { get; init; }
    public required string Version { get; init; }
    public required TimeSpan Elapsed { get; init; }
    public string? Defects { get; init; }
    public UserId? AssignedtoId { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement> Properties { get; set; } = [];
}
