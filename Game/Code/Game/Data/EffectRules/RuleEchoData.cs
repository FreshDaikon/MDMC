using Godot;
using Mdmc.Code.Game.Combat.ArsenalSystem.EffectStack;
using Mdmc.Code.Game.Combat.ArsenalSystem.EffectStack.Rules;
using Mdmc.Code.System;

namespace Mdmc.Code.Game.Data.EffectRules;

[GlobalClass]
public partial class RuleEchoData: EffectRuleData
{
    [Export]
    public MD.SkillType TypeToEcho { get; set; }
    
    public override Rule GetRule()
    {
        var newRule = new RuleEcho
        {
            Charges = Charges,
            TriggerEffectData = EffectData,
            TypeToEcho = TypeToEcho,
        };
        return newRule;
    }
}