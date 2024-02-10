namespace Daikon.Contracts.Users;

public record AuthUserResponse(
    Guid SessionToken
);