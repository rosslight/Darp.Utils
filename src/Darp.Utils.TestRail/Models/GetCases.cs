namespace Darp.Utils.TestRail.Models;

public sealed record GetCases(int Offset, int Limit, int Size, IEnumerable<GetCaseResponse> Cases);
