namespace Darp.Utils.TestRail.Models;

public sealed record GetRuns(int Offset, int Limit, int Size, IEnumerable<GetRun> Runs);
