namespace Daikon.Contracts.Users;

public record AuthUserRequest
{
    public required string SteamTicket { get; init;} 
};
