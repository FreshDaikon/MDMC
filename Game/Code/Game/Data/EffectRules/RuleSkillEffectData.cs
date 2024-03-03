using Daikon.Helpers;
using Godot;

namespace Daikon.Game.EffectRules.Rules;

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