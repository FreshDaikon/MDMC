namespace Daikon.Contracts.Games;

public record JoinGameRequest(
    Guid SessionToken,
    string JoinCode
);
