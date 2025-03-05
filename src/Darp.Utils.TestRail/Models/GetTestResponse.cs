namespace Darp.Utils.TestRail.Models;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary> The model of the test </summary>
public sealed record GetTestResponse
{
    /// <summary> The ID of the user the test is assigned to </summary>
    public int? AssignedToId { get; init; }

    /// <summary> The ID of the related test case </summary>
    public CaseId CaseId { get; init; }

    /// <summary> The estimate of the related test case, e.g. “30s” or “1m 45s” </summary>
    public TimeSpan? Estimate { get; init; }

    /// <summary> The estimated forecast of the related test case, e.g. “30s” or “1m 45s” </summary>
    public TimeSpan? EstimateForecast { get; init; }

    /// <summary> The unique ID of the test </summary>
    public required TestId Id { get; init; }

    /// <summary> The ID of the milestone that is linked to the test case </summary>
    public MilestoneId? MilestoneId { get; init; }

    /// <summary> The ID of the priority that is linked to the test case </summary>
    public PriorityId? PriorityId { get; init; }

    /// <summary> A comma-separated list of references/requirements that are linked to the test case </summary>
    public string? Refs { get; init; }

    /// <summary> The ID of the test run the test belongs to </summary>
    public RunId? RunId { get; init; }

    /// <summary> The ID of the current status of the test, also see get_statuses </summary>
    public StatusId StatusId { get; init; }

    /// <summary> The title of the related test case </summary>
    public required string Title { get; init; }

    /// <summary> The ID of the test case type that is linked to the test case </summary>
    public TypeId TypeId { get; init; }

    /// <summary> User defined properties </summary>
    [JsonExtensionData]
#pragma warning disable CA2227 // Collection properties should be read only is ignored because the json serializes requires a settable collection
    public Dictionary<string, JsonElement> Properties { get; set; } = [];
#pragma warning restore CA2227
}
