namespace Daikon.Contracts.Users;

public record AuthUserRequest
{
    public string SteamTicket { get; init;} 
};
