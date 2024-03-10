using Mdmc.Code.Game.Data.Decorators;
using Mdmc.Code.System;

namespace Mdmc.Code.Game.Combat.ArsenalSystem.EffectStack.Rules;

public class RuleEcho: EffectRule
{
    //Stuff..
    public MD.SkillType TypeToEcho { get; init; }
    
    public override void TryResolve()
    {
        base.TryResolve();

        if (!OriginSkill.TriggerSkill().SUCCESS) return;
        
        SetWasResolved(true);
    }
    
    public override bool CheckCondition()
    {
        if (IsConditional && !PreviousOutcome)
            return false;

        return TriggerSkill != null && TriggerSkill.SkillType == TypeToEcho;
    }

    public override EffectData GetEffect()
    {
        TriggerEffectData.ExtraData = new
        {
            Type = EffectData.DataType.FreeCast
        };
        return base.GetEffect();
    }
}