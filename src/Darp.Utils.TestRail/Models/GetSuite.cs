namespace Darp.Utils.TestRail.Models;

using System.Diagnostics.CodeAnalysis;

/// <summary> The model for the suite </summary>
public sealed record GetSuite
{
    /// <summary> The date/time when the test suite was closed (as UNIX timestamp) (added with TestRail 4.0) </summary>
    public DateTimeOffset? CompletedOn { get; init; }

    /// <summary> The description of the test suite </summary>
    public string? Description { get; init; }

    /// <summary> True if the test suite is a baseline test suite and false otherwise (added with TestRail 4.0) </summary>
    public bool IsBaseline { get; init; }

    /// <summary> True if the test suite is marked as completed/archived and false otherwise (added with TestRail 4.0) </summary>
    [MemberNotNullWhen(true, nameof(CompletedOn))]
    public bool IsCompleted { get; init; }

    /// <summary> True if the test suite is a master test suite and false otherwise (added with TestRail 4.0) </summary>
    public bool IsMaster { get; init; }

    /// <summary> The unique ID of the test suite </summary>
    public SuiteId Id { get; init; }

    /// <summary> The name of the test suite </summary>
    public required string Name { get; init; }

    /// <summary> The ID of the project this test suite belongs to </summary>
    public ProjectId ProjectId { get; init; }

    /// <summary> The address/URL of the test suite in the user interface </summary>
    public string? Url { get; init; }
}
