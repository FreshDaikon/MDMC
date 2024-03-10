using Godot;
using Mdmc.Code.Game.Combat.ArsenalSystem.EffectStack;
using Mdmc.Code.Game.Combat.ArsenalSystem.EffectStack.Rules;
using Mdmc.Code.System;

namespace Mdmc.Code.Game.Data.EffectRules;

[GlobalClass]
public partial class RuleSkillEffectData: EffectRuleData
{
    [Export]
    public MD.SkillType TypeToEffect { get; set; }
    
    public override EffectRule GetRule()
    {
        var newRule = new RuleSkillEffect()
        {
            TriggerEffectData = EffectData,
            TypeToEffect = TypeToEffect,
        };
        return newRule;
    }
}