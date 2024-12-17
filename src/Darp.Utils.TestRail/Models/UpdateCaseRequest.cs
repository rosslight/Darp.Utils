namespace Darp.Utils.TestRail.Models;

using System.Text.Json.Serialization;

/// <summary> The model for updating the case </summary>
public sealed class UpdateCaseRequest
{
    /// <summary> The ID of the test case </summary>
    public required CaseId CaseId { get; set; }

    /// <summary> The ID of the section the test case should be updated to </summary>
    public SectionId? SectionId { get; set; }

    /// <summary> The title of the test case </summary>
    public string? Title { get; set; }

    /// <summary> The ID of the template (field layout)—requires TestRail 5.2 or later </summary>
    public TemplateId? TemplateId { get; set; }

    /// <summary> The ID of the case type </summary>
    public TypeId? TypeId { get; set; }

    /// <summary> The ID of the case priority </summary>
    public PriorityId? PriorityId { get; set; }

    /// <summary> The estimate, e.g. “30s” or “1m 45s” </summary>
    public TimeSpan? Estimate { get; set; }

    /// <summary> The ID of the milestone to link to the test case </summary>
    public MilestoneId? MilestoneId { get; set; }

    /// <summary> A comma-separated list of references/requirements </summary>
    public string? Refs { get; set; }

    /// <summary> User defined properties </summary>
    [JsonExtensionData]
#pragma warning disable CA2227 // Collection properties should be read only is ignored because the json serializes requires a settable collection
    public Dictionary<string, object> Properties { get; set; } = [];
#pragma warning restore CA2227
}
