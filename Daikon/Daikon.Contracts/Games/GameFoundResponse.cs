namespace Daikon.Contracts.Games;

public record GameFoundReponse
{
    public required string ServerHost { get; init;}
    public required int ServerPort { get; init; }
}