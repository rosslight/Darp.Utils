namespace Darp.Utils.TestRail.Models;

/// <summary> The model for a run </summary>
public sealed record GetRun
{
    /// <summary> The date/time when the test run was closed (as UNIX timestamp) </summary>
    public DateTimeOffset? CompletedOn { get; init; }

    /// <summary> The unique ID of the test run </summary>
    public required RunId Id { get; init; }

    /// <summary> True if the test run was closed and false otherwise </summary>
    public bool IsCompleted { get; init; }

    /// <summary> The ID of the milestone this test run belongs to </summary>
    public MilestoneId? MilestoneId { get; init; }

    /// <summary> The name of the test run </summary>
    public required string Name { get; init; }

    /// <summary> The ID of the project this test run belongs to </summary>
    public required ProjectId ProjectId { get; init; }
}
