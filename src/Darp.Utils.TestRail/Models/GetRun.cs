namespace Darp.Utils.TestRail.Models;

public sealed record GetRun(
    RunId Id,
    string Name,
    ProjectId ProjectId,
    MilestoneId? MilestoneId,
    bool IsCompleted,
    DateTimeOffset? CompletedOn
);
