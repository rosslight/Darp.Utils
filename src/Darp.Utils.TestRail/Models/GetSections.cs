namespace Darp.Utils.TestRail.Models;

/// <summary> The GetSections model </summary>
/// <param name="Offset"> The offset </param>
/// <param name="Limit"> The limit </param>
/// <param name="Size"> The size </param>
/// <param name="Sections"> The sections </param>
public sealed record GetSections(int Offset, int Limit, int Size, IEnumerable<GetSection> Sections);
