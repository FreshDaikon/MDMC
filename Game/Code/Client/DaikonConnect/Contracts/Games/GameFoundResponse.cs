namespace Daikon.Contracts.Games;

public record GameFoundReponse
{
    public string ServerHost { get; init;}
    public int ServerPort { get; init; }
}