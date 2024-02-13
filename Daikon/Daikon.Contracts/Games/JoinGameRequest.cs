namespace Daikon.Contracts.Games;

public record JoinGameRequest
{
    public required Guid SessionToken { get; init; }
    public required string JoinCode { get; init; }
};
