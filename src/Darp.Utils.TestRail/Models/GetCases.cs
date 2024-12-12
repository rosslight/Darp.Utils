namespace Darp.Utils.TestRail.Models;

/// <summary> The GetCases model </summary>
/// <param name="Offset"> The offset </param>
/// <param name="Limit"> The limit </param>
/// <param name="Size"> The size </param>
/// <param name="Cases"> The cases </param>
public sealed record GetCases(int Offset, int Limit, int Size, IEnumerable<GetCaseResponse> Cases);
