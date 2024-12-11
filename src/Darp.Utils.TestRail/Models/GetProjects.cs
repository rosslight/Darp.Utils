namespace Darp.Utils.TestRail.Models;

public sealed record GetProjects(int Offset, int Limit, int Size, IEnumerable<GetProject> Projects);
