using System.Collections.Generic;
using Daikon.Contracts.Models;
using Daikon.Game;

namespace Daikon.Server.Database.Models;

public record ServerArenaRecord
{
    public required int ArenaId { get; init; }
    public required float Progress { get; init; }
    public required double Runtime { get; init; }
    public required double Time { get; init; }
    public required List<ArenaParticipant> Players { get; init; }
}