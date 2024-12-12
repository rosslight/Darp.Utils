namespace Darp.Utils.TestRail.Models;

/// <summary> The GetRuns model </summary>
/// <param name="Offset"> The offset </param>
/// <param name="Limit"> The limit </param>
/// <param name="Size"> The size </param>
/// <param name="Runs"> The runs </param>
public sealed record GetRuns(int Offset, int Limit, int Size, IEnumerable<GetRun> Runs);
