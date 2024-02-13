namespace Daikon.Contracts.Games;

public record GameCreatedResponse
{
    public required string JoinCode { get; init; }
    public required string ServerHost { get; init;}
    public required int ServerPort { get; init; }
}