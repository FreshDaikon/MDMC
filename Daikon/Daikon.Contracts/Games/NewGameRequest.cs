namespace Daikon.Contracts.Games;

public record NewGameRequest 
{
    public required Guid SessionToken { get; init; }
    public required string Arena { get; init;}
};
    

