namespace Darp.Utils.TestRail.Models;

/// <summary> The filter when getting a project </summary>
public enum ProjectsFilter
{
    /// <summary> Get all projects </summary>
    All = -1,

    /// <summary> Get active projects only </summary>
    ActiveProjectsOnly = 0,

    /// <summary> Get completed projects only </summary>
    CompletedProjectsOnly = 1,
}
