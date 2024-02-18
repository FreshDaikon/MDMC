namespace Daikon.Contracts.Games;

public record GameCreatedResponse
{
    public string JoinCode { get; set; }
    public string ServerHost { get; set;}
    public int ServerPort { get; set; }
}