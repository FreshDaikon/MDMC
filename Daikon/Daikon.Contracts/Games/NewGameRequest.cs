namespace Daikon.Contracts.Games;

public record NewGameRequest (
    Guid SessionToken,
    string Arena
);
    

