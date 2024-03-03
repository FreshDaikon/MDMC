using Daikon.Helpers;
using Godot;

namespace Daikon.Game.EffectRules.Rules;

[GlobalClass]
public partial class RuleConditionEffect: EffectRuleData
{
    [Export]
    public MD.SkillType TypeToEcho { get; set; }
    
    public override EffectRule GetRule()
    {
        var newRule = new RuleEcho
        {
            TriggerEffectData = EffectData,
            TypeToEcho = TypeToEcho,
        };
        return newRule;
    }
}