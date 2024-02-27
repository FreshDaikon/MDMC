namespace Daikon.Contracts.Users;

public record AdminCleanup
{
    public required string Key { get; init;}  
    public required string Repo { get; init;} 
};
