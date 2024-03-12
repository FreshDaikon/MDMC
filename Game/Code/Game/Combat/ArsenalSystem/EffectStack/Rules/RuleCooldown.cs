using Mdmc.Code.Game.Data.Decorators;
using Mdmc.Code.System;

namespace Mdmc.Code.Game.Combat.ArsenalSystem.EffectStack.Rules;

public class RuleCooldown: Rule
{
    public override bool CheckCondition()
    {
        return TriggerSkill != null && TriggerSkill.Cooldown > 0;
    }
}