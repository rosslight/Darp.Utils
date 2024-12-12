namespace Darp.Utils.TestRail.Models;

/// <summary> The user model </summary>
public sealed record GetUser
{
    /// <summary> The unique ID of the user </summary>
    public required UserId Id { get; init; }

    /// <summary> The email address of the user as configured in TestRail </summary>
    public required string Email { get; init; }

    /// <summary> The full name of the user </summary>
    public required string Name { get; init; }
}
