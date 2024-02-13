namespace Daikon.Contracts.Users;

public record AuthUserResponse
{
    public required Guid SessionToken { get; init; }
};