namespace Daikon.Contracts.Models;

public record ArenaParticipant
{
    public required int Id { get; init;}
    public required string Name { get; init; }
    public required double Dps { get; init; }
    public required double Hps { get; init; }
    public required double Deaths { get; init; }
    public required ParticipantBuild Build { get; init;}
}