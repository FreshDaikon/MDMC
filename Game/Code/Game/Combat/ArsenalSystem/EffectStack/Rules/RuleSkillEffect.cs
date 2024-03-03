using Daikon.Helpers;

namespace Daikon.Game;

public class RuleCooldownReduction: EffectRule
{
    public MD.SkillType TypeToReduce { get; init; }
    
    public override void TryResolve()
    {
        base.TryResolve();
        SetWasResolved(true);
    }
    
    public override bool CheckCondition()
    {
        if (IsConditional && !PreviousOutcome)
            return false;

        return TriggerSkill != null && TriggerSkill.SkillType == TypeToReduce;
    }
}