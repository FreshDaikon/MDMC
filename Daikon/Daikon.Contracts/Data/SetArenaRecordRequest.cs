using Daikon.Contracts.Models;

namespace Daikon.Contracts.Data;

public record SetArenRecordRequest()
{
    public required string ServerKey { get; init; }
    public required int Id { get; init; }
    public required Guid Session { get; init; }
    public required float Progress { get; init; }
    public required double DateTime {get; init; }
    public required double Runtime { get; init; }
    public required ArenaParticipant[] Participants { get; init;}
}