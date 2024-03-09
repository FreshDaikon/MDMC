namespace Daikon.Contracts.Models;

public record ParticipantBuild
{   
    public required int MainId { get; init;}
    public required int LeftId { get; init; }
    public required int RightId { get; init; }
    public required int[] MainSkillIds { get; init;}
    public required int[] LeftSkillIds { get; init;}
    public required int[] RightSkillIds { get; init;}
}