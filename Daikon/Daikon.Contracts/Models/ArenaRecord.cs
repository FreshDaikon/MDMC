namespace Daikon.Contracts.Models;

public record ArenaRecord
{
    public required Guid Session { get; init; }
    public required float Progress { get; init; }
    public required double DateTime {get; init; }
    public required double Runtime { get; init; }
    public required ArenaParticipant[] Participants { get; init;}
}