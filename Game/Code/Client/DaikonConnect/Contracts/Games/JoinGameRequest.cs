using System;

namespace Daikon.Contracts.Games;

public record JoinGameRequest
{
    public Guid SessionToken { get; init; }
    public string JoinCode { get; init; }
};
