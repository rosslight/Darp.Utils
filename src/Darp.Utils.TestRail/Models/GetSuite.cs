namespace Darp.Utils.TestRail.Models;

using System.Diagnostics.CodeAnalysis;

#pragma warning disable CA1054
#pragma warning disable CA1056

public sealed record GetSuite(
    SuiteId Id,
    string Name,
    string Description,
    ProjectId ProjectId,
    bool IsBaseline,
    DateTimeOffset? CompletedOn,
    bool IsCompleted,
    bool IsMaster,
    string Url
)
{
    /// <summary>  </summary>
    [MemberNotNullWhen(true, nameof(CompletedOn))]
    public bool IsCompleted { get; init; } = IsCompleted;
}
