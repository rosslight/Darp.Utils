namespace Darp.Utils.TestRail.Models;

/// <summary> The suite mode </summary>
public enum SuiteMode
{
    /// <summary> Get a single suite </summary>
    SingleSuite = 1,

    /// <summary> Get a single suite and baselines </summary>
    SingleSuiteAndBaselines = 2,

    /// <summary> Multiple suites </summary>
    MultiSuites = 3,
}
