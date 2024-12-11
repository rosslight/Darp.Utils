namespace Darp.Utils.TestRail.Models;

#pragma warning disable CA1054
#pragma warning disable CA1056

public sealed record GetProject(ProjectId Id, string Name, bool IsCompleted, SuiteMode SuiteMode, string Url);
