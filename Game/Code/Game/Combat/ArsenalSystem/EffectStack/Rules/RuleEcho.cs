using Daikon.Helpers;
using Godot;

namespace Daikon.Game;

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
}