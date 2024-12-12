namespace Darp.Utils.TestRail.Models;

/// <summary> Model for getting a milestone </summary>
public sealed record GetMilestone
{
    /// <summary> The unique ID of the milestone </summary>
    public required MilestoneId Id { get; init; }

    /// <summary> The name of the milestone </summary>
    public required string Name { get; init; }

    /// <summary> The ID of the project the milestone belongs to </summary>
    public required ProjectId ProjectId { get; init; }
}
