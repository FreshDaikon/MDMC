using Daikon.Helpers;

namespace Daikon.Game;

public class RuleSkillEffect: EffectRule
{
    public MD.SkillType TypeToEffect { get; init; }
    
    public override void TryResolve()
    {
        base.TryResolve();
        SetWasResolved(true);
    }
    
    public override bool CheckCondition()
    {
        if (IsConditional && !PreviousOutcome)
            return false;

        return TriggerSkill != null && TriggerSkill.SkillType == TypeToEffect;
    }
}