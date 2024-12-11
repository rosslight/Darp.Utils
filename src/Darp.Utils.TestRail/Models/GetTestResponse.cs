namespace Darp.Utils.TestRail.Models;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

public sealed record GetTestResponse(
    int? AssignedToId,
    CaseId CaseId,
    TimeSpan? Estimate,
    TimeSpan? EstimateForecast,
    TestId Id,
    MilestoneId? MilestoneId,
    PriorityId? PriorityId,
    string Refs,
    RunId? RunId,
    StatusId StatusId,
    string Title,
    TypeId TypeId
)
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement> Properties { get; set; } = [];
}
