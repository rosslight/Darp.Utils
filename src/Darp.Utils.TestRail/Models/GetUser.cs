namespace Darp.Utils.TestRail.Models;

public sealed record GetUser
{
    public required UserId Id { get; init; }
    public required string Email { get; init; }
    public required string Name { get; init; }
}
