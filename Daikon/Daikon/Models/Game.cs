namespace Daikon.Models.Games;

public record Game
{
    public required string SessionId { get; init; }
    public required string JoinCode { get; init; }
    public required string ServerHost { get; init; }
    public required int ServerPort { get; init; }
    //
    public DateTime LastHeartBeat {get; set; }
     
}

