namespace Darp.Utils.TestRail.Models;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary> Model for adding a new result </summary>
public sealed record AddResultRequest
{
    /// <summary> The ID of a user the test should be assigned to </summary>
    public UserId? AssignedtoId { get; init; }

    /// <summary> The comment/description for the test result </summary>
    public required string Comment { get; init; }

    /// <summary> A comma-separated list of defects to link to the test result </summary>
    public string? Defects { get; init; }

    /// <summary> The time it took to execute the test </summary>
    public required TimeSpan Elapsed { get; init; }

    /// <summary> The ID of the test status </summary>
    public required StatusId StatusId { get; init; }

    /// <summary> The version or build you tested against </summary>
    public required string Version { get; init; }

    /// <summary> Custom fields </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement> Properties { get; set; } = [];
}
