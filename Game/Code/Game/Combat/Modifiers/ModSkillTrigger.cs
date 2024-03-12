using Mdmc.Code.Game.Combat.ArsenalSystem;

namespace Mdmc.Code.Game.Combat.Modifiers.SkillTriggers;

public class ModSkillTrigger
{

    public enum TriggerType
    {
        HasCooldown,
        HasCastTime,
        IsSkillType,
    }

    public enum TriggerEffect
    {
        ReduceCooldown,
        IncreaseResource,
        ReduceCastTime,
        IncreasePotency,
    }
    public required TriggerType Type { get; init; }
    public required TriggerEffect Effect { get; init; }
    public System.MD.SkillType TypeToCheck { get; init;}
    public float EffectValue { get; init; }

    public bool CheckSkill(Skill skill)
    {
        bool check = Type switch
        {
            TriggerType.HasCooldown => skill.Cooldown > 0,
            TriggerType.HasCastTime => skill.CastTime > 0,
            TriggerType.IsSkillType => skill.SkillType == TypeToCheck,
            _ => false
        };
        return check;
    }

    public ModTriggerEffct GetModifierEffect()
    {
        var newEffect = new ModTriggerEffct()
        {
            Effect = Effect,
            EffectValue = EffectValue,
        };
        return newEffect;
    }


}