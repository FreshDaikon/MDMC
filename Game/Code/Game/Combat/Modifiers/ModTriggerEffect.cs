using static Mdmc.Code.Game.Combat.Modifiers.SkillTriggers.ModSkillTrigger;

public record ModTriggerEffct
{
    public required TriggerEffect Effect { get; init; }
    public float EffectValue { get; init; }
}