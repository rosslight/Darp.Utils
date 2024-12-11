namespace Darp.Utils.TestRail.Models;

public sealed record GetSections(int Offset, int Limit, int Size, IEnumerable<GetSection> Sections);
