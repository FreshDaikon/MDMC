namespace Daikon.Contracts.Data;

public record GetArenaRecordsRequest
{
    public required Guid SessionToken { get; init; }
    public required int ArenaId { get; init; }
}