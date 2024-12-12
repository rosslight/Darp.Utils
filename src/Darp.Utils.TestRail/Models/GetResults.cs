namespace Darp.Utils.TestRail.Models;

/// <summary> The result model </summary>
public sealed record GetResults
{
    /// <summary> The unique ID of the test result </summary>
    public required ResultId Id { get; init; }
}
