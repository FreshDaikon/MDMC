using Godot;
using Mdmc.Code.Game.Combat.ArsenalSystem.EffectStack;
using Mdmc.Code.Game.Combat.ArsenalSystem.EffectStack.Rules;

namespace Mdmc.Code.Game.Data.EffectRules;

[GlobalClass]
public partial class RuleCooldownData: EffectRuleData
{
   
    public override Rule GetRule()
    {
        var newCombo = new RuleCooldown()
        {
            Charges = Charges,
            TriggerEffectData = EffectData,
        };
        return newCombo;
    }
}