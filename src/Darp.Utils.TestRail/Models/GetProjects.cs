namespace Darp.Utils.TestRail.Models;

/// <summary> The GetProjects model </summary>
/// <param name="Offset"> The offset </param>
/// <param name="Limit"> The limit </param>
/// <param name="Size"> The size </param>
/// <param name="Projects"> The projects </param>
public sealed record GetProjects(int Offset, int Limit, int Size, IEnumerable<GetProject> Projects);
