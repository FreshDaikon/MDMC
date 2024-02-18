using System;

namespace Daikon.Contracts.Games;

public record NewGameRequest 
{
    public Guid SessionToken { get; init; }
    public string Arena { get; init;}
};
    

