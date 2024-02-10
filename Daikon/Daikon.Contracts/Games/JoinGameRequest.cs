namespace Daikon.Contracts.Games;

public record JoinGameRequest(
    Guid AuthToken,
    string JoinCode
);
