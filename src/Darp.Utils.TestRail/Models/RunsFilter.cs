namespace Darp.Utils.TestRail.Models;

/// <summary> The filter when getting a run </summary>
public enum RunsFilter
{
    /// <summary> Get all runs </summary>
    All = -1,

    /// <summary> Get active runs only </summary>
    ActiveRunsOnly = 0,

    /// <summary> Get completed runs only </summary>
    CompletedRunsOnly = 1,
}
