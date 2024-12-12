namespace Darp.Utils.TestRail.Models;

/// <summary> The ID of a TestRail Status </summary>
public enum StatusId
{
    /// <summary> No ID is defined </summary>
    None = 0,

    /// <summary> The Passed state </summary>
    Passed = 1,

    /// <summary> The Blocked state </summary>
    Blocked = 2,

    /// <summary> The Untested state </summary>
    Untested = 3,

    /// <summary> The Retest state </summary>
    Retest = 4,

    /// <summary> The Failed state </summary>
    Failed = 5,
}
