namespace Darp.Utils.TestRail.Models;

/// <summary> The GetTests model </summary>
/// <param name="Offset"> The offset </param>
/// <param name="Limit"> The limit </param>
/// <param name="Size"> The size </param>
/// <param name="Tests"> The tests </param>
public sealed record GetTests(int Offset, int Limit, int Size, IEnumerable<GetTestResponse> Tests);
