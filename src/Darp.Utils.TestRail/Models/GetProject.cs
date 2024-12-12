namespace Darp.Utils.TestRail.Models;

/// <summary> Model for getting a project </summary>
public sealed record GetProject
{
    /// <summary> The unique ID of the project </summary>
    public required ProjectId Id { get; init; }

    /// <summary> True if the project is marked as completed and false otherwise </summary>
    public bool IsCompleted { get; init; }

    /// <summary> The name of the project </summary>
    public required string Name { get; init; }

    /// <summary> The suite mode of the project </summary>
    public required SuiteMode SuiteMode { get; init; }

    /// <summary> The address/URL of the project in the user interface </summary>
    public required string Url { get; init; }
}
