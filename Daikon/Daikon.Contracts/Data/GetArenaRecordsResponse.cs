using Daikon.Contracts.Models;

namespace Daikon.Contracts.Data;

public record GetArenaRecordsResponse
{
    public required ArenaRecord[] Records { get; init; }
}