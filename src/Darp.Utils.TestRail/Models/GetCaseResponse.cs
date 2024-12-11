namespace Darp.Utils.TestRail.Models;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json;

/// <summary> Details about TestRail Case </summary>
public sealed record GetCaseResponse
{
    /// <summary> The unique ID of the test case </summary>
    public required CaseId Id { get; init; }

    /// <summary> The title of the test case  </summary>
    public required string Title { get; init; }

    /// <summary> The ID of the test case type that is linked to the test case </summary>
    public required TypeId TypeId { get; init; }

    /// <summary> The estimate, e.g. “30s” or “1m 45s” </summary>
    public required TimeSpan? Estimate { get; init; }

    /// <summary> The estimate forecast, e.g. “30s” or “1m 45s” </summary>
    public required TimeSpan? EstimateForecast { get; init; }

    /// <summary> The ID of the user who created the test case </summary>
    public required UserId CreatedBy { get; init; }

    /// <summary> The date/time when the test case was created (as UNIX timestamp) </summary>
    public required DateTimeOffset CreatedOn { get; init; }

    /// <summary> The ID of the user who last updated the test case </summary>
    public required UserId UpdatedBy { get; init; }

    /// <summary> The date/time when the test case was last updated (as UNIX timestamp) </summary>
    public required DateTimeOffset UpdatedOn { get; init; }

    /// <summary> The ID of the suite the test case belongs to </summary>
    public required SuiteId SuiteId { get; init; }

    /// <summary> The display order </summary>
    public int? DisplayOrder { get; init; }

    /// <summary> The ID of the milestone that is linked to the test case </summary>
    public required MilestoneId? MilestoneId { get; init; }

    /// <summary> The ID of the priority that is linked to the test case </summary>
    public required int PriorityId { get; init; }

    /// <summary> A comma-separated list of references/requirements that are linked to the test case </summary>
    public required string? Refs { get; init; }

    /// <summary> The ID of the template (field layout) the test case uses—requires TestRail 5.2 or later </summary>
    public required TemplateId? TemplateId { get; init; }

    /// <summary> The ID of the section the test case belongs to </summary>
    public required SectionId SectionId { get; init; }

    /// <summary> True, if the case is deleted </summary>
    [JsonConverter(typeof(BoolIntJsonConverter))]
    public bool IsDeleted { get; init; }

    /// <summary> Unknown properties </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement> Properties { get; set; } = [];
}
