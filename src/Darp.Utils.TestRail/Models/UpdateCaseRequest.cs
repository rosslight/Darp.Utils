namespace Darp.Utils.TestRail.Models;

using System.Text.Json.Serialization;

public sealed class UpdateCaseRequest
{
    public required CaseId CaseId { get; set; }
    public SectionId? SectionId { get; set; }
    public string? Title { get; set; }
    public TemplateId? TemplateId { get; set; }
    public TypeId? TypeId { get; set; }
    public PriorityId? PriorityId { get; set; }
    public TimeSpan? Estimate { get; set; }
    public MilestoneId? MilestoneId { get; set; }
    public string? Refs { get; set; }

    [JsonExtensionData]
    public Dictionary<string, object> Properties { get; init; } = [];
}
