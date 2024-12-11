namespace Darp.Utils.TestRail.Models;

public sealed record GetTests(int Offset, int Limit, int Size, GetTestResponse[] Tests);
